using Lab2.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Reflection;
using System.Text;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// 1. Настройка сервисов
builder.Services.AddDbContext<AdvertisementServiceContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddMemoryCache();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options => {
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true; // ИСПРАВЛЕНО: добавлено .Cookie
});

var app = builder.Build();
app.UseSession();


int N = 25;
int cacheTime = 2 * N + 240;

// 2. Предварительное кэширование
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AdvertisementServiceContext>();
    var cache = scope.ServiceProvider.GetRequiredService<IMemoryCache>();
    var cacheOptions = new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromSeconds(cacheTime));

    try
    {
        cache.Set("users", context.Users.Take(20).ToList(), cacheOptions);
        cache.Set("advertisements", context.Advertisements.Take(20).ToList(), cacheOptions);
        cache.Set("categories", context.Categories.Take(20).ToList(), cacheOptions);
    }
    catch { /* Если базы нет при первом запуске */ }
}

// 3. Middleware
app.Use(async (context, next) =>
{
    var path = context.Request.Path.Value?.ToLower();
    var db = context.RequestServices.GetRequiredService<AdvertisementServiceContext>();
    context.Response.ContentType = "text/html; charset=utf-8";

    if (string.IsNullOrEmpty(path) || path == "/")
    {
        await context.Response.WriteAsync("<h1>Меню</h1><ul>" +
            "<li><a href='/info'>Инфо (/info)</a></li>" +
            "<li><a href='/table/users'>Таблица Users</a></li>" +
            "<li><a href='/searchform1'>Форма 1 (Ads)</a></li>" +
            "<li><a href='/searchform2'>Форма 2 (Users)</a></li></ul>");
        return;
    }

    if (path == "/info")
    {
        await context.Response.WriteAsync($"<h2>Info</h2>IP: {context.Connection.RemoteIpAddress}<br>Agent: {context.Request.Headers["User-Agent"]}");
        return;
    }

    if (path.StartsWith("/table/"))
    {
        var tableName = path.Split('/').Last();
        var cache = context.RequestServices.GetRequiredService<IMemoryCache>();
        if (cache.TryGetValue(tableName, out object? list) && list != null)
            await context.Response.WriteAsync($"<h2>{tableName}</h2>" + HtmlHelper.ToHtmlTable((System.Collections.IEnumerable)list));
        else
            await context.Response.WriteAsync("Данные не найдены в кэше.");
        return;
    }

    if (path == "/searchform1" || path == "/searchform2")
    {
        // Логика сохранения (POST)
        if (context.Request.Method == "POST")
        {
            var data = new
            {
                Text = context.Request.Form["q"].ToString(),
                Select = context.Request.Form["cat"].ToString(),
                Radio = context.Request.Form["sort"].ToString()
            };
            string json = JsonSerializer.Serialize(data);
            if (path == "/searchform1") context.Response.Cookies.Append("FormState1", json);
            else context.Session.SetString("FormState2", json);
        }

        // Логика загрузки (2.4)
        string? stateJson = (path == "/searchform1")
            ? context.Session.GetString("FormState2")
            : context.Request.Cookies["FormState1"];

        // Безопасно извлекаем значения
        string q = "", cat = "", sort = "asc";
        if (!string.IsNullOrEmpty(stateJson))
        {
            using var doc = JsonDocument.Parse(stateJson);
            q = doc.RootElement.GetProperty("Text").GetString() ?? "";
            cat = doc.RootElement.GetProperty("Select").GetString() ?? "";
            sort = doc.RootElement.GetProperty("Radio").GetString() ?? "asc";
        }

        // Генерируем HTML формы (ИСПРАВЛЕНО: убраны сложные вложения кавычек)
        var formHtml = new StringBuilder();
        formHtml.Append($"<h2>{(path == "/searchform1" ? "Поиск объявлений" : "Поиск пользователей")}</h2>");
        formHtml.Append("<form method='post'>");
        formHtml.Append($"Текст: <input type='text' name='q' value='{q}'><br><br>");

        if (path == "/searchform1")
        {
            formHtml.Append("Категория: <select name='cat'><option value='0'>Все</option>");
            foreach (var c in db.Categories)
                formHtml.Append($"<option value='{c.CategoryId}' {(cat == c.CategoryId.ToString() ? "selected" : "")}>{c.CategoryName}</option>");
            formHtml.Append("</select>");
        }
        else
        {
            formHtml.Append("Рейтинг: <select name='cat'>");
            formHtml.Append($"<option value='0' {(cat == "0" ? "selected" : "")}>Любой</option>");
            formHtml.Append($"<option value='4' {(cat == "4" ? "selected" : "")}>4+</option>");
            formHtml.Append("</select>");
        }

        formHtml.Append("<br><br>Сортировка: ");
        formHtml.Append($"<input type='radio' name='sort' value='asc' {(sort == "asc" ? "checked" : "")}> A-Z ");
        formHtml.Append($"<input type='radio' name='sort' value='desc' {(sort == "desc" ? "checked" : "")}> Z-A ");
        formHtml.Append("<br><br><input type='submit' value='Найти'></form><hr>");

        await context.Response.WriteAsync(formHtml.ToString());

        // Вывод результатов поиска
        if (context.Request.Method == "POST" || !string.IsNullOrEmpty(q))
        {
            if (path == "/searchform1")
            {
                var query = db.Advertisements.AsQueryable();
                if (!string.IsNullOrEmpty(q)) query = query.Where(a => a.Title.Contains(q));
                if (int.TryParse(cat, out int cid) && cid > 0) query = query.Where(a => a.CategoryId == cid);
                var res = (sort == "asc" ? query.OrderBy(a => a.Title) : query.OrderByDescending(a => a.Title)).Take(10).ToList();
                await context.Response.WriteAsync(HtmlHelper.ToHtmlTable(res));
            }
            else
            {
                var query = db.Users.AsQueryable();
                if (!string.IsNullOrEmpty(q)) query = query.Where(u => u.Username.Contains(q));
                if (decimal.TryParse(cat, out decimal r) && r > 0) query = query.Where(u => u.Rating >= r);
                var res = (sort == "asc" ? query.OrderBy(u => u.Username) : query.OrderByDescending(u => u.Username)).Take(10).ToList();
                await context.Response.WriteAsync(HtmlHelper.ToHtmlTable(res));
            }
        }
        await context.Response.WriteAsync("<br><a href='/'>Назад</a>");
        return;
    }
    await next();
});

app.Run(async c => { c.Response.StatusCode = 404; await c.Response.WriteAsync("404 Not Found"); });
app.Run();

// Помощник для таблиц
public static class HtmlHelper
{
    public static string ToHtmlTable(System.Collections.IEnumerable list)
    {
        var sb = new StringBuilder("<table border='1' style='border-collapse:collapse;width:100%'>");
        bool header = false;
        foreach (var item in list)
        {
            var props = item.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.PropertyType.IsPrimitive || p.PropertyType == typeof(string) || p.PropertyType == typeof(decimal)).ToList();
            if (!header)
            {
                sb.Append("<tr>");
                foreach (var p in props) sb.Append($"<th>{p.Name}</th>");
                sb.Append("</tr>");
                header = true;
            }
            sb.Append("<tr>");
            foreach (var p in props) sb.Append($"<td>{p.GetValue(item) ?? "null"}</td>");
            sb.Append("</tr>");
        }
        return header ? sb.Append("</table>").ToString() : "Нет данных";
    }
}