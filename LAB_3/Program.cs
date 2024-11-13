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
            // ��������� ����������� ��� ������� � �� � �������������� EF
            string connectionString = builder.Configuration.GetConnectionString("DefaultConnection");


            ////������� ������ ����������� � ���������� ���������� SQL Server, ���������� ��� ������������ � ������
            //// ������� ������������ ��� ���������� ��������� ����������
            //IConfigurationRoot configuration = builder.Configuration.AddUserSecrets<Program>().Build();
            //connectionString = configuration.GetConnectionString("RemoteSQLConnection");
            ////��������� ������ � ��� ������������ �� secrets.json
            //string secretPass = configuration["Database:password"];
            //string secretUser = configuration["Database:login"];
            //SqlConnectionStringBuilder sqlConnectionStringBuilder = new(connectionString)
            //{
            //    Password = secretPass,
            //    UserID = secretUser
            //};
            //connectionString = sqlConnectionStringBuilder.ConnectionString;



            services.AddDbContext<ProductionContext>(options => options.UseSqlServer(connectionString));

            // ���������� �����������
            services.AddMemoryCache();

            // ���������� ��������� ������
            services.AddDistributedMemoryCache();
            services.AddSession();

            // ��������� ����������� CachedProductsService
            services.AddScoped<ICachedProductsService, CachedProductsService>();
            services.AddScoped<ICachedEnterprisesService, CachedEnterprisesService>();
            services.AddScoped<ICachedProductionPlansService, CachedProductionPlansService>();
            services.AddScoped<ICachedProductTypesService, CachedProductTypesService>();
            services.AddScoped<ICachedSalesPlansService, CachedSalesPlansService>();

            //������������� MVC - ���������
            //services.AddControllersWithViews();
            var app = builder.Build();


            // ��������� ��������� ����������� ������
            app.UseStaticFiles();

            // ��������� ��������� ������
            app.UseSession();

            // ��������� ����������� ��������� middleware �� ������������� ���� ������ � ���������� �� �������������
            app.UseDbInitializer();


            //����������� � Session ��������, ��������� � �����
           


            //����������� � �ookies ��������, ��������� � �����
            //..


            // ����� ���������� � �������
            app.Map("/info", (appBuilder) =>
            {
                appBuilder.Run(async (context) =>
                {
                    // ������������ ������ ��� ������ 
                    string strResponse = "<HTML><HEAD><TITLE>����������</TITLE></HEAD>" +
                    "<META http-equiv='Content-Type' content='text/html; charset=utf-8'/>" +
                    "<BODY><H1>����������:</H1>";
                    strResponse += "<BR> ������: " + context.Request.Host;
                    strResponse += "<BR> ����: " + context.Request.PathBase;
                    strResponse += "<BR> ��������: " + context.Request.Protocol;
                    strResponse += "<BR><A href='/'>�������</A></BODY></HTML>";
                    // ����� ������
                    await context.Response.WriteAsync(strResponse);
                });
            });
            app.Map("/searchform1", (appBuilder) =>
            {
                appBuilder.Run(async (context) =>
                {
                    // ������ �������� �� cookies (���� ��� ����)
                    string nameSearch = context.Request.Cookies["Name"] ?? "";
                    // ������������ ������ ��� ������ ������������ HTML �����
                    string strResponse = "<HTML><HEAD><TITLE>������������</TITLE></HEAD>" +
                    "<META http-equiv='Content-Type' content='text/html; charset=utf-8'/>" +
                    "<BODY><FORM action ='/searchform1' / >" +
                    "���:<BR><INPUT type = 'text' name = 'Name' value = " + nameSearch + ">" +
                    "<BR><BR><INPUT type ='submit' value='��������� � ����'><INPUT type ='submit' value='��������'></FORM>";

                    // ��������, ���� �� ���������� ����� �������� �� �����
                    if (context.Request.Query.ContainsKey("Name"))
                    {
                        // ��������� �������� �� ���������� �������
                        nameSearch = context.Request.Query["Name"];
                        // ��������, ���� �� ���������� ����� �������� �� �����
                        if (context.Request.Query.ContainsKey("Name"))
                        {
                            // ��������� �������� �� ���������� �������
                            nameSearch = context.Request.Query["Name"];

                            // ����� �������� � �������
                            int N = 29;
                            // ������ �������� � cookies
                            context.Response.Cookies.Append("Name", nameSearch, new CookieOptions
                            {
                                Expires = DateTimeOffset.UtcNow.AddSeconds(2 * N + 240)
                            });

                            // ��������� ������ �� ������ ������
                            var cachedEnterprisesService = context.RequestServices.GetService<ICachedProductsService>();
                            var product = cachedEnterprisesService.SearchObj(nameSearch);
                            if (product != null)
                            {
                                context.Session.Set<Product>("product", product);
                                strResponse += "<TABLE BORDER=1>" +
                                "<TR>" +
                                "<TH>ID products</TH>" +
                                "<TH>���</TH>" +
                                "<TH>��������������</TH>" +
                                "<TH>Unit</TH>" +
                                "<TH>����</TH>" +
                                "</TR>" +
                                "<TR>" +
                                "<TD>" + product.ProductId + "</TD>" +
                                "<TD>" + product.Name + "</TD>" +
                                "<TD>" + product.Characteristics + "</TD>" +
                                "<TD>" + product.Unit + "</TD>" +
                                "<TD>" + product.Photo + "</TD>" +
                                "</TR>" +
                                "</TABLE>" +
                                "<BR><A href='/'>�������</A></BR>" +
                                "<BR><A href='/product'>���������</A></BR>" +
                                "<BR><A href='/\'>������ ������������</A></BR>" +
                                "</BODY></HTML>";
                            }
                        }
                    }
                    strResponse += "<BR><A href='/'>�������</A></BODY></HTML>";
                    // ����������� ����� ������������ HTML �����
                    await context.Response.WriteAsync(strResponse);
                });
            });
            app.Map("/searchform2", (appBuilder) =>
            {
                appBuilder.Run(async (context) =>
                {

                    // ���������� �� Session ������� User
                    Product product = context.Session.Get<Product>("product") ?? new Product();
                    string name = product.Name;
                    // ������������ ������ ��� ������ ������������ HTML �����
                    string strResponse = "<HTML><HEAD><TITLE>������������</TITLE></HEAD>" +
                    "<META http-equiv='Content-Type' content='text/html; charset=utf-8'/>" +
                    "<BODY><FORM action ='/searchform2' / >" +
                    "���:<BR><INPUT type = 'text' name = 'Name' value = " + name + ">" +
                    "<BR><BR><INPUT type ='submit' value='��������� � Session'><INPUT type ='submit' value='��������'></FORM>";
                    strResponse += "<BR><A href='/'>�������</A></BODY></HTML>";

                    // ������ � Session ������ ������� User
                    name = context.Request.Query["Name"];
                    ICachedProductsService cachedEnterprisesService = context.RequestServices.GetService<ICachedProductsService>();
                    product = cachedEnterprisesService.SearchObj(name);
                    if (product != null)
                    {
                        context.Session.Set<Product>("product", product);
                        strResponse += "<TABLE BORDER=1>" +
                        "<TR>" +
                        "<TH>ID products</TH>" +
                        "<TH>���</TH>" +
                        "<TH>��������������</TH>" +
                        "<TH>Unit</TH>" +
                        "<TH>����</TH>" +
                        "</TR>" +
                        "<TR>" +
                        "<TD>" + product.ProductId + "</TD>" +
                        "<TD>" + product.Name + "</TD>" +
                        "<TD>" + product.Characteristics + "</TD>" +
                        "<TD>" + product.Unit + "</TD>" +
                        "<TD>" + product.Photo + "</TD>" +
                        "</TR>" +
                        "</TABLE>" +
                        "<BR><A href='/'>�������</A></BR>" +
                        "<BR><A href='/product'>���������</A></BR>" +
                        "<BR><A href='/\'>������ ������������</A></BR>" +
                        "</BODY></HTML>";
                    }
                    // ����������� ����� ������������ HTML �����
                    await context.Response.WriteAsync(strResponse);
                });
            });
            ;

            // ����� ������������ ���������� �� ������� ���� ������
            app.Map("/products", (appBuilder) =>
            {
                appBuilder.Run(async (context) =>
                {
                    //��������� � �������
                    ICachedProductsService cachedProductsService = context.RequestServices.GetService<ICachedProductsService>();
                    IEnumerable<Product> products = cachedProductsService.GetProducts("Products20");
                    string HtmlString = "<HTML><HEAD><TITLE>���������</TITLE></HEAD>" +
                    "<META http-equiv='Content-Type' content='text/html; charset=utf-8'/>" +
                    "<BODY><H1>������ ���������</H1>" +
                    "<TABLE BORDER=1>";
                    HtmlString += "<TR>";
                    HtmlString += "<TH>ID products</TH>";
                    HtmlString += "<TH>���</TH>";
                    HtmlString += "<TH>��������������</TH>";
                    HtmlString += "<TH>Unit</TH>";
                    HtmlString += "<TH>����</TH>";
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
                    HtmlString += "<BR><A href='/'>�������</A></BR>";
                    HtmlString += "<BR><A href='/product'>���������</A></BR>";
                    HtmlString += "<BR><A href='/\'>������ ������������</A></BR>";
                    HtmlString += "</BODY></HTML>";

                    // ����� ������
                    await context.Response.WriteAsync(HtmlString);
                });
            });
            app.Map("/enterprises", (appBuilder) =>
            {
                appBuilder.Run(async (context) =>
                {
                    //��������� � �������
                    ICachedEnterprisesService cachedEnterprisesService = context.RequestServices.GetService<ICachedEnterprisesService>();
                    IEnumerable<Enterprise> enterprises = cachedEnterprisesService.GetEnterprises("Enterprises20");
                    string HtmlString = "<HTML><HEAD><TITLE>�����������</TITLE></HEAD>" +
                    "<META http-equiv='Content-Type' content='text/html; charset=utf-8'/>" +
                    "<BODY><H1>������ �����������</H1>" +
                    "<TABLE BORDER=1>";
                    HtmlString += "<TR>";
                    HtmlString += "<TH>ID Enterprise</TH>";
                    HtmlString += "<TH>���</TH>";
                    HtmlString += "<TH>��� ���������</TH>";
                    HtmlString += "<TH>��� ����������</TH>";
                    HtmlString += "<TH>����� �������������</TH>";
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
                    HtmlString += "<BR><A href='/'>�������</A></BR>";
                    HtmlString += "<BR><A href='/product'>���������</A></BR>";
                    HtmlString += "<BR><A href='/form'>������ ������������</A></BR>";
                    HtmlString += "</BODY></HTML>";

                    // ����� ������
                    await context.Response.WriteAsync(HtmlString);
                });
            });

            app.Map("/productionPlans", (appBuilder) =>
            {
                appBuilder.Run(async (context) =>
                {
                    //��������� � �������
                    ICachedProductionPlansService cachedProductionPlansService = context.RequestServices.GetService<ICachedProductionPlansService>();
                    IEnumerable<ProductionPlan> productionPlans = cachedProductionPlansService.GetProductionPlans("ProductionPlans20");
                    string HtmlString = "<HTML><HEAD><TITLE>���������������� ����</TITLE></HEAD>" +
                    "<META http-equiv='Content-Type' content='text/html; charset=utf-8'/>" +
                    "<BODY><H1>���������������� �����</H1>" +
                    "<TABLE BORDER=1>";
                    HtmlString += "<TR>";
                    HtmlString += "<TH>ID ProductionPlan</TH>";
                    HtmlString += "<TH>ID Enterprise</TH>";
                    HtmlString += "<TH>ID Product</TH>";
                    HtmlString += "<TH>��������������� �����</TH>";
                    HtmlString += "<TH>����������� �����</TH>";
                    HtmlString += "<TH>Quarter</TH>";
                    HtmlString += "<TH>���</TH>";
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
                    HtmlString += "<BR><A href='/'>�������</A></BR>";
                    HtmlString += "<BR><A href='/product'>���������</A></BR>";
                    HtmlString += "<BR><A href='/form'>������ ������������</A></BR>";
                    HtmlString += "</BODY></HTML>";

                    // ����� ������
                    await context.Response.WriteAsync(HtmlString);
                });
            });
            app.Map("/productTypes", (appBuilder) =>
            {
                appBuilder.Run(async (context) =>
                {
                    //��������� � �������
                    ICachedProductTypesService cachedProductTypesService = context.RequestServices.GetService<ICachedProductTypesService>();
                    IEnumerable<ProductType> productTypes = cachedProductTypesService.GetProductTypes("ProductTypes20");
                    string HtmlString = "<HTML><HEAD><TITLE>���� ���������</TITLE></HEAD>" +
                    "<META http-equiv='Content-Type' content='text/html; charset=utf-8'/>" +
                    "<BODY><H1>������ ����� ���������</H1>" +
                    "<TABLE BORDER=1>";
                    HtmlString += "<TR>";
                    HtmlString += "<TH>ID ProductType</TH>";
                    HtmlString += "<TH>���</TH>";
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
                    HtmlString += "<BR><A href='/'>�������</A></BR>";
                    HtmlString += "<BR><A href='/product'>���������</A></BR>";
                    HtmlString += "<BR><A href='/form'>������ ������������</A></BR>";
                    HtmlString += "</BODY></HTML>";

                    // ����� ������
                    await context.Response.WriteAsync(HtmlString);
                });
            });
            app.Map("/salesPlans", (appBuilder) =>
            {
                appBuilder.Run(async (context) =>
                {
                    //��������� � �������
                    ICachedSalesPlansService cachedSalesPlansService = context.RequestServices.GetService<ICachedSalesPlansService>();
                    IEnumerable<SalesPlan> salesPlans = cachedSalesPlansService.GetSalesPlans("SalesPlans20");
                    string HtmlString = "<HTML><HEAD><TITLE>���� ������ ������</TITLE></HEAD>" +
                    "<META http-equiv='Content-Type' content='text/html; charset=utf-8'/>" +
                    "<BODY><H1>����� ������</H1>" +
                    "<TABLE BORDER=1>";
                    HtmlString += "<TR>";
                    HtmlString += "<TH>ID SalesPlan</TH>";
                    HtmlString += "<TH>ID Enterprise</TH>";
                    HtmlString += "<TH>ID Product</TH>";
                    HtmlString += "<TH>��������������� �������</TH>";
                    HtmlString += "<TH>����������� �������</TH>";
                    HtmlString += "<TH>Quarter</TH>";
                    HtmlString += "<TH>���</TH>";
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
                    HtmlString += "<BR><A href='/'>�������</A></BR>";
                    HtmlString += "<BR><A href='/product'>���������</A></BR>";
                    HtmlString += "<BR><A href='/form'>������ ������������</A></BR>";
                    HtmlString += "</BODY></HTML>";

                    // ����� ������
                    await context.Response.WriteAsync(HtmlString);
                });
            });



            // ��������� �������� � ����������� ������ ������� �� web-�������
            app.Run((context) =>
            {
                //��������� � �������
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

                string HtmlString = "<HTML><HEAD><TITLE>������� ��������</TITLE></HEAD>" +
                "<META http-equiv='Content-Type' content='text/html; charset=utf-8'/>" +
                "<BODY><H1>�������</H1>";
                HtmlString += "<H2>������ �������� � ��� �������</H2>";
                HtmlString += "<BR><A href='/'>�������</A></BR>";
                HtmlString += "<BR><A href='/products'>���������</A></BR>";
                HtmlString += "<BR><A href='/enterprises'>�����������</A></BR>";
                HtmlString += "<BR><A href='/productionPlans'>���������������� �����</A></BR>";
                HtmlString += "<BR><A href='/productTypes'>���� ���������</A></BR>";
                HtmlString += "<BR><A href='/salesPlans'>����� ������</A></BR>";
                HtmlString += "<BR><A href='/searchform1'>����� 1</A></BR>";
                HtmlString += "<BR><A href='/searchform2'>����� 2</A></BR>";
                HtmlString += "</BODY></HTML>";

                return context.Response.WriteAsync(HtmlString);

            });

            //������������� MVC - ���������
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