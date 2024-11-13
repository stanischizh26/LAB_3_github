using LAB_3.Infrastructure;
using LAB_3.Middleware;
using LAB_3.Models;
using LAB_3.Services.CachedService;
using LAB_3.Services.ICachedService;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LAB_3
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            var services = builder.Services;
            // внедрение зависимости для доступа к БД с использованием EF
            string connectionString = builder.Configuration.GetConnectionString("DefaultConnection");


            ////Вариант строки подключения к экземпляру удаленного SQL Server, требующего имя пользователя и пароль
            //// создаем конфигурацию для считывания секретной информации
            //IConfigurationRoot configuration = builder.Configuration.AddUserSecrets<Program>().Build();
            //connectionString = configuration.GetConnectionString("RemoteSQLConnection");
            ////Считываем пароль и имя пользователя из secrets.json
            //string secretPass = configuration["Database:password"];
            //string secretUser = configuration["Database:login"];
            //SqlConnectionStringBuilder sqlConnectionStringBuilder = new(connectionString)
            //{
            //    Password = secretPass,
            //    UserID = secretUser
            //};
            //connectionString = sqlConnectionStringBuilder.ConnectionString;



            services.AddDbContext<ProductionContext>(options => options.UseSqlServer(connectionString));

            // добавление кэширования
            services.AddMemoryCache();

            // добавление поддержки сессии
            services.AddDistributedMemoryCache();
            services.AddSession();

            // внедрение зависимости CachedProductsService
            services.AddScoped<ICachedProductsService, CachedProductsService>();
            services.AddScoped<ICachedEnterprisesService, CachedEnterprisesService>();
            services.AddScoped<ICachedProductionPlansService, CachedProductionPlansService>();
            services.AddScoped<ICachedProductTypesService, CachedProductTypesService>();
            services.AddScoped<ICachedSalesPlansService, CachedSalesPlansService>();

            //Использование MVC - отключено
            //services.AddControllersWithViews();
            var app = builder.Build();


            // добавляем поддержку статических файлов
            app.UseStaticFiles();

            // добавляем поддержку сессий
            app.UseSession();

            // добавляем собственный компонент middleware по инициализации базы данных и производим ее инициализацию
            app.UseDbInitializer();


            //Запоминание в Session значений, введенных в форме
           


            //Запоминание в Сookies значений, введенных в форме
            //..


            // Вывод информации о клиенте
            app.Map("/info", (appBuilder) =>
            {
                appBuilder.Run(async (context) =>
                {
                    // Формирование строки для вывода 
                    string strResponse = "<HTML><HEAD><TITLE>Информация</TITLE></HEAD>" +
                    "<META http-equiv='Content-Type' content='text/html; charset=utf-8'/>" +
                    "<BODY><H1>Информация:</H1>";
                    strResponse += "<BR> Сервер: " + context.Request.Host;
                    strResponse += "<BR> Путь: " + context.Request.PathBase;
                    strResponse += "<BR> Протокол: " + context.Request.Protocol;
                    strResponse += "<BR><A href='/'>Главная</A></BODY></HTML>";
                    // Вывод данных
                    await context.Response.WriteAsync(strResponse);
                });
            });
            app.Map("/searchform1", (appBuilder) =>
            {
                appBuilder.Run(async (context) =>
                {
                    // Чтение значения из cookies (если оно есть)
                    string nameSearch = context.Request.Cookies["Name"] ?? "";
                    // Формирование строки для вывода динамической HTML формы
                    string strResponse = "<HTML><HEAD><TITLE>Пользователь</TITLE></HEAD>" +
                    "<META http-equiv='Content-Type' content='text/html; charset=utf-8'/>" +
                    "<BODY><FORM action ='/searchform1' / >" +
                    "Имя:<BR><INPUT type = 'text' name = 'Name' value = " + nameSearch + ">" +
                    "<BR><BR><INPUT type ='submit' value='Сохранить в Куки'><INPUT type ='submit' value='Показать'></FORM>";

                    // Проверка, было ли отправлено новое значение из формы
                    if (context.Request.Query.ContainsKey("Name"))
                    {
                        // Получение значения из параметров запроса
                        nameSearch = context.Request.Query["Name"];
                        // Проверка, было ли отправлено новое значение из формы
                        if (context.Request.Query.ContainsKey("Name"))
                        {
                            // Получение значения из параметров запроса
                            nameSearch = context.Request.Query["Name"];

                            // Номер варианта в журнале
                            int N = 29;
                            // Запись значения в cookies
                            context.Response.Cookies.Append("Name", nameSearch, new CookieOptions
                            {
                                Expires = DateTimeOffset.UtcNow.AddSeconds(2 * N + 240)
                            });

                            // Получение данных из службы поиска
                            var cachedEnterprisesService = context.RequestServices.GetService<ICachedProductsService>();
                            var product = cachedEnterprisesService.SearchObj(nameSearch);
                            if (product != null)
                            {
                                context.Session.Set<Product>("product", product);
                                strResponse += "<TABLE BORDER=1>" +
                                "<TR>" +
                                "<TH>ID products</TH>" +
                                "<TH>Имя</TH>" +
                                "<TH>Характеристики</TH>" +
                                "<TH>Unit</TH>" +
                                "<TH>Фото</TH>" +
                                "</TR>" +
                                "<TR>" +
                                "<TD>" + product.ProductId + "</TD>" +
                                "<TD>" + product.Name + "</TD>" +
                                "<TD>" + product.Characteristics + "</TD>" +
                                "<TD>" + product.Unit + "</TD>" +
                                "<TD>" + product.Photo + "</TD>" +
                                "</TR>" +
                                "</TABLE>" +
                                "<BR><A href='/'>Главная</A></BR>" +
                                "<BR><A href='/product'>Продукция</A></BR>" +
                                "<BR><A href='/\'>Данные пользователя</A></BR>" +
                                "</BODY></HTML>";
                            }
                        }
                    }
                    strResponse += "<BR><A href='/'>Главная</A></BODY></HTML>";
                    // Асинхронный вывод динамической HTML формы
                    await context.Response.WriteAsync(strResponse);
                });
            });
            app.Map("/searchform2", (appBuilder) =>
            {
                appBuilder.Run(async (context) =>
                {

                    // Считывание из Session объекта User
                    Product product = context.Session.Get<Product>("product") ?? new Product();
                    string name = product.Name;
                    // Формирование строки для вывода динамической HTML формы
                    string strResponse = "<HTML><HEAD><TITLE>Пользователь</TITLE></HEAD>" +
                    "<META http-equiv='Content-Type' content='text/html; charset=utf-8'/>" +
                    "<BODY><FORM action ='/searchform2' / >" +
                    "Имя:<BR><INPUT type = 'text' name = 'Name' value = " + name + ">" +
                    "<BR><BR><INPUT type ='submit' value='Сохранить в Session'><INPUT type ='submit' value='Показать'></FORM>";
                    strResponse += "<BR><A href='/'>Главная</A></BODY></HTML>";

                    // Запись в Session данных объекта User
                    name = context.Request.Query["Name"];
                    ICachedProductsService cachedEnterprisesService = context.RequestServices.GetService<ICachedProductsService>();
                    product = cachedEnterprisesService.SearchObj(name);
                    if (product != null)
                    {
                        context.Session.Set<Product>("product", product);
                        strResponse += "<TABLE BORDER=1>" +
                        "<TR>" +
                        "<TH>ID products</TH>" +
                        "<TH>Имя</TH>" +
                        "<TH>Характеристики</TH>" +
                        "<TH>Unit</TH>" +
                        "<TH>Фото</TH>" +
                        "</TR>" +
                        "<TR>" +
                        "<TD>" + product.ProductId + "</TD>" +
                        "<TD>" + product.Name + "</TD>" +
                        "<TD>" + product.Characteristics + "</TD>" +
                        "<TD>" + product.Unit + "</TD>" +
                        "<TD>" + product.Photo + "</TD>" +
                        "</TR>" +
                        "</TABLE>" +
                        "<BR><A href='/'>Главная</A></BR>" +
                        "<BR><A href='/product'>Продукция</A></BR>" +
                        "<BR><A href='/\'>Данные пользователя</A></BR>" +
                        "</BODY></HTML>";
                    }
                    // Асинхронный вывод динамической HTML формы
                    await context.Response.WriteAsync(strResponse);
                });
            });
            ;

            // Вывод кэшированной информации из таблицы базы данных
            app.Map("/products", (appBuilder) =>
            {
                appBuilder.Run(async (context) =>
                {
                    //обращение к сервису
                    ICachedProductsService cachedProductsService = context.RequestServices.GetService<ICachedProductsService>();
                    IEnumerable<Product> products = cachedProductsService.GetProducts("Products20");
                    string HtmlString = "<HTML><HEAD><TITLE>Продукция</TITLE></HEAD>" +
                    "<META http-equiv='Content-Type' content='text/html; charset=utf-8'/>" +
                    "<BODY><H1>Список продукции</H1>" +
                    "<TABLE BORDER=1>";
                    HtmlString += "<TR>";
                    HtmlString += "<TH>ID products</TH>";
                    HtmlString += "<TH>Имя</TH>";
                    HtmlString += "<TH>Характеристики</TH>";
                    HtmlString += "<TH>Unit</TH>";
                    HtmlString += "<TH>Фото</TH>";
                    HtmlString += "</TR>";
                    foreach (var product in products)
                    {
                        HtmlString += "<TR>";
                        HtmlString += "<TD>" + product.ProductId + "</TD>";
                        HtmlString += "<TD>" + product.Name + "</TD>";
                        HtmlString += "<TD>" + product.Characteristics + "</TD>";
                        HtmlString += "<TD>" + product.Unit + "</TD>";
                        HtmlString += "<TD>" + product.Photo + "</TD>"; 
                        HtmlString += "</TR>";
                    }
                    HtmlString += "</TABLE>";
                    HtmlString += "<BR><A href='/'>Главная</A></BR>";
                    HtmlString += "<BR><A href='/product'>Продукция</A></BR>";
                    HtmlString += "<BR><A href='/\'>Данные пользователя</A></BR>";
                    HtmlString += "</BODY></HTML>";

                    // Вывод данных
                    await context.Response.WriteAsync(HtmlString);
                });
            });
            app.Map("/enterprises", (appBuilder) =>
            {
                appBuilder.Run(async (context) =>
                {
                    //обращение к сервису
                    ICachedEnterprisesService cachedEnterprisesService = context.RequestServices.GetService<ICachedEnterprisesService>();
                    IEnumerable<Enterprise> enterprises = cachedEnterprisesService.GetEnterprises("Enterprises20");
                    string HtmlString = "<HTML><HEAD><TITLE>Предприятия</TITLE></HEAD>" +
                    "<META http-equiv='Content-Type' content='text/html; charset=utf-8'/>" +
                    "<BODY><H1>Список предприятий</H1>" +
                    "<TABLE BORDER=1>";
                    HtmlString += "<TR>";
                    HtmlString += "<TH>ID Enterprise</TH>";
                    HtmlString += "<TH>Имя</TH>";
                    HtmlString += "<TH>Имя директора</TH>";
                    HtmlString += "<TH>Тип активности</TH>";
                    HtmlString += "<TH>Форма собственности</TH>";
                    HtmlString += "</TR>";
                    foreach (var enterprise in enterprises)
                    {
                        HtmlString += "<TR>";
                        HtmlString += "<TD>" + enterprise.EnterpriseId + "</TD>";
                        HtmlString += "<TD>" + enterprise.Name + "</TD>";
                        HtmlString += "<TD>" + enterprise.DirectorName + "</TD>";
                        HtmlString += "<TD>" + enterprise.ActivityType + "</TD>";
                        HtmlString += "<TD>" + enterprise.OwnershipForm + "</TD>";
                        HtmlString += "</TR>";
                    }
                    HtmlString += "</TABLE>";
                    HtmlString += "<BR><A href='/'>Главная</A></BR>";
                    HtmlString += "<BR><A href='/product'>Продукция</A></BR>";
                    HtmlString += "<BR><A href='/form'>Данные пользователя</A></BR>";
                    HtmlString += "</BODY></HTML>";

                    // Вывод данных
                    await context.Response.WriteAsync(HtmlString);
                });
            });

            app.Map("/productionPlans", (appBuilder) =>
            {
                appBuilder.Run(async (context) =>
                {
                    //обращение к сервису
                    ICachedProductionPlansService cachedProductionPlansService = context.RequestServices.GetService<ICachedProductionPlansService>();
                    IEnumerable<ProductionPlan> productionPlans = cachedProductionPlansService.GetProductionPlans("ProductionPlans20");
                    string HtmlString = "<HTML><HEAD><TITLE>Производственный план</TITLE></HEAD>" +
                    "<META http-equiv='Content-Type' content='text/html; charset=utf-8'/>" +
                    "<BODY><H1>Производственные планы</H1>" +
                    "<TABLE BORDER=1>";
                    HtmlString += "<TR>";
                    HtmlString += "<TH>ID ProductionPlan</TH>";
                    HtmlString += "<TH>ID Enterprise</TH>";
                    HtmlString += "<TH>ID Product</TH>";
                    HtmlString += "<TH>Запланированный объем</TH>";
                    HtmlString += "<TH>Фактический объем</TH>";
                    HtmlString += "<TH>Quarter</TH>";
                    HtmlString += "<TH>Год</TH>";
                    HtmlString += "</TR>";
                    foreach (var productionPlan in productionPlans)
                    {
                        HtmlString += "<TR>";
                        HtmlString += "<TD>" + productionPlan.ProductionPlanId + "</TD>";
                        HtmlString += "<TD>" + productionPlan.EnterpriseId + "</TD>";
                        HtmlString += "<TD>" + productionPlan.ProductId + "</TD>";
                        HtmlString += "<TD>" + productionPlan.PlannedVolume + "</TD>";
                        HtmlString += "<TD>" + productionPlan.ActualVolume + "</TD>";
                        HtmlString += "<TD>" + productionPlan.Quarter + "</TD>";
                        HtmlString += "<TD>" + productionPlan.Year + "</TD>";
                        HtmlString += "</TR>";
                    }
                    HtmlString += "</TABLE>";
                    HtmlString += "<BR><A href='/'>Главная</A></BR>";
                    HtmlString += "<BR><A href='/product'>Продукция</A></BR>";
                    HtmlString += "<BR><A href='/form'>Данные пользователя</A></BR>";
                    HtmlString += "</BODY></HTML>";

                    // Вывод данных
                    await context.Response.WriteAsync(HtmlString);
                });
            });
            app.Map("/productTypes", (appBuilder) =>
            {
                appBuilder.Run(async (context) =>
                {
                    //обращение к сервису
                    ICachedProductTypesService cachedProductTypesService = context.RequestServices.GetService<ICachedProductTypesService>();
                    IEnumerable<ProductType> productTypes = cachedProductTypesService.GetProductTypes("ProductTypes20");
                    string HtmlString = "<HTML><HEAD><TITLE>Типы продукции</TITLE></HEAD>" +
                    "<META http-equiv='Content-Type' content='text/html; charset=utf-8'/>" +
                    "<BODY><H1>Список типов продукции</H1>" +
                    "<TABLE BORDER=1>";
                    HtmlString += "<TR>";
                    HtmlString += "<TH>ID ProductType</TH>";
                    HtmlString += "<TH>Имя</TH>";
                    HtmlString += "<TH>ID Product</TH>";
                    HtmlString += "</TR>";
                    foreach (var productType in productTypes)
                    {
                        HtmlString += "<TR>";
                        HtmlString += "<TD>" + productType.ProductTypeId + "</TD>";
                        HtmlString += "<TD>" + productType.Name + "</TD>";
                        HtmlString += "<TD>" + productType.ProductId + "</TD>";
                        HtmlString += "</TR>";
                    }
                    HtmlString += "</TABLE>";
                    HtmlString += "<BR><A href='/'>Главная</A></BR>";
                    HtmlString += "<BR><A href='/product'>Продукция</A></BR>";
                    HtmlString += "<BR><A href='/form'>Данные пользователя</A></BR>";
                    HtmlString += "</BODY></HTML>";

                    // Вывод данных
                    await context.Response.WriteAsync(HtmlString);
                });
            });
            app.Map("/salesPlans", (appBuilder) =>
            {
                appBuilder.Run(async (context) =>
                {
                    //обращение к сервису
                    ICachedSalesPlansService cachedSalesPlansService = context.RequestServices.GetService<ICachedSalesPlansService>();
                    IEnumerable<SalesPlan> salesPlans = cachedSalesPlansService.GetSalesPlans("SalesPlans20");
                    string HtmlString = "<HTML><HEAD><TITLE>Типы планов продаж</TITLE></HEAD>" +
                    "<META http-equiv='Content-Type' content='text/html; charset=utf-8'/>" +
                    "<BODY><H1>Планы продаж</H1>" +
                    "<TABLE BORDER=1>";
                    HtmlString += "<TR>";
                    HtmlString += "<TH>ID SalesPlan</TH>";
                    HtmlString += "<TH>ID Enterprise</TH>";
                    HtmlString += "<TH>ID Product</TH>";
                    HtmlString += "<TH>Запланированные продажи</TH>";
                    HtmlString += "<TH>Фактические продажи</TH>";
                    HtmlString += "<TH>Quarter</TH>";
                    HtmlString += "<TH>Год</TH>";
                    HtmlString += "</TR>";
                    foreach (var salesPlan in salesPlans)
                    {
                        HtmlString += "<TR>";
                        HtmlString += "<TD>" + salesPlan.SalesPlanId + "</TD>";
                        HtmlString += "<TD>" + salesPlan.EnterpriseId + "</TD>";
                        HtmlString += "<TD>" + salesPlan.ProductId + "</TD>";
                        HtmlString += "<TD>" + salesPlan.PlannedSales + "</TD>";
                        HtmlString += "<TD>" + salesPlan.ActualSales + "</TD>";
                        HtmlString += "<TD>" + salesPlan.Quarter + "</TD>";
                        HtmlString += "<TD>" + salesPlan.Year + "</TD>";

                        HtmlString += "</TR>";
                    }
                    HtmlString += "</TABLE>";
                    HtmlString += "<BR><A href='/'>Главная</A></BR>";
                    HtmlString += "<BR><A href='/product'>Продукция</A></BR>";
                    HtmlString += "<BR><A href='/form'>Данные пользователя</A></BR>";
                    HtmlString += "</BODY></HTML>";

                    // Вывод данных
                    await context.Response.WriteAsync(HtmlString);
                });
            });



            // Стартовая страница и кэширование данных таблицы на web-сервере
            app.Run((context) =>
            {
                //обращение к сервису
                ICachedProductsService cachedProductsService = context.RequestServices.GetService<ICachedProductsService>();
                cachedProductsService.AddProducts("Products20");

                ICachedEnterprisesService cachedEnterprisesService = context.RequestServices.GetService<ICachedEnterprisesService>();
                cachedEnterprisesService.AddEnterprises("Enterprises20");

                ICachedProductionPlansService cachedProductionPlansService = context.RequestServices.GetService<ICachedProductionPlansService>();
                cachedProductionPlansService.AddProductionPlans("ProductionPlans20");

                ICachedProductTypesService cachedProductTypesService = context.RequestServices.GetService<ICachedProductTypesService>();
                cachedProductTypesService.AddProductTypes("ProductTypes20");

                ICachedSalesPlansService cachedSalesPlansService = context.RequestServices.GetService<ICachedSalesPlansService>();
                cachedSalesPlansService.AddSalesPlans("SalesPlans20");

                string HtmlString = "<HTML><HEAD><TITLE>Сходные продукты</TITLE></HEAD>" +
                "<META http-equiv='Content-Type' content='text/html; charset=utf-8'/>" +
                "<BODY><H1>Главная</H1>";
                HtmlString += "<H2>Данные записаны в кэш сервера</H2>";
                HtmlString += "<BR><A href='/'>Главная</A></BR>";
                HtmlString += "<BR><A href='/products'>Продукция</A></BR>";
                HtmlString += "<BR><A href='/enterprises'>Предприятия</A></BR>";
                HtmlString += "<BR><A href='/productionPlans'>Производственные планы</A></BR>";
                HtmlString += "<BR><A href='/productTypes'>Типы продуктов</A></BR>";
                HtmlString += "<BR><A href='/salesPlans'>Планы продаж</A></BR>";
                HtmlString += "<BR><A href='/searchform1'>Поиск 1</A></BR>";
                HtmlString += "<BR><A href='/searchform2'>Поиск 2</A></BR>";
                HtmlString += "</BODY></HTML>";

                return context.Response.WriteAsync(HtmlString);

            });

            //Использование MVC - отключено
            //app.UseRouting();
            //app.UseAuthorization();
            //app.UseEndpoints(endpoints =>
            //{
            //    endpoints.MapControllerRoute(
            //        name: "default",
            //        pattern: "{controller=Home}/{action=Index}/{id?}");
            //});

            app.Run();
        }
    }
}