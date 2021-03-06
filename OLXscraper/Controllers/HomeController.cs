﻿using HtmlAgilityPack;
using OLXscraper.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace OLXscraper.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = $"Your contact page.";

            return View();
        }

        public ActionResult Hello(string id)
        {
            return Content("Hello" + id);
        }

        public ActionResult Scraper(string id)
        {
            ProductsList productsList = new ProductsList();

            return View(GetProductListFromOlx(productsList, id));
        }

        public static ProductsList GetProductListFromOlx(ProductsList productsList, string str)
        {
            string url = ($"https://www.olx.pl/elektronika/komputery/katowice/q- {str} /?search%5Border%5D=filter_float_price%3Aasc&search%5Bdist%5D=5").Replace(" ", "");
            string urlPage2 = ($"https://www.olx.pl/elektronika/komputery/katowice/q- {str} /?search%5Border%5D=filter_float_price%3Aasc&search%5Bdist%5D=5").Replace(" ", "");

            List<string> urlList = new List<string>();

            for (int i = 1; i < 30; i++)
            {
                urlPage2 = ($"https://www.olx.pl/elektronika/komputery/katowice/q- {str} /?search%5Border%5D=filter_float_price%3Aasc&search%5Bdist%5D=5&page = {i}").Replace(" ", "");
                urlList.Add(urlPage2);
            }

            productsList = new ProductsList();
            productsList.MyList = new List<Product>();

            Parallel.ForEach(urlList, (urlAdress, state) =>
            {
                var httpClient = new HttpClient();
                var html = httpClient.GetStringAsync(urlAdress);

                var htmlDocument = new HtmlDocument();
                htmlDocument.LoadHtml(html.Result);


                var ProductList = htmlDocument.DocumentNode.Descendants(0)
                    .Where(n => n.HasClass("offer")).ToList();

                var sb = new StringBuilder();

                var test = htmlDocument.DocumentNode.Descendants("link")
                            .Select(node => node.GetAttributeValue("href", ""))
                            .FirstOrDefault();

                if (!test.Contains("search"))
                {
                    state.Break();
                }

                foreach (var ProductListItem in ProductList)
                {

                    string title;
                    string price;
                    string link;
                    string image;

                    try
                    {
                        title = ProductListItem.Descendants("a")
                            .Where(node => node.GetAttributeValue("data-cy", "")
                            .Equals("listing-ad-title")).FirstOrDefault()
                            .InnerHtml.Trim('\r', '\t', '\n');

                        price = ProductListItem.Descendants("p")
                            .Where(node => node.GetAttributeValue("class", "")
                            .Equals("price")).FirstOrDefault()
                            .InnerHtml.Trim('\r', '\t', '\n');

                        link = ProductListItem.Descendants("a")
                            .Select(node => node.GetAttributeValue("href", ""))
                            .FirstOrDefault().Trim();

                        image = ProductListItem.Descendants("img")
                            .Select(node => node.GetAttributeValue("src", ""))
                            .FirstOrDefault().Trim();
                    }
                    catch (Exception)
                    {
                        continue;
                    }

                    var newProduct = new Product()
                    {
                        Title = title.ToString().Trim('\r', '\t', '\n').Replace("<strong>", "").Replace("</strong>", "").Replace("&quot;", ""),
                        Price = price.ToString().Trim('\r', '\t', '\n').Replace("<strong>", "").Replace("</strong>", ""),
                        Link = link.ToString().Trim('\r', '\t', '\n'),
                        Image = image.ToString().Trim('\r', '\t', '\n')
                    };
                    try
                    {
                        if (productsList.MyList.Any(x => x.Title == newProduct.Title))
                            continue;
                    }
                    catch (Exception)
                    {

                        continue;
                    }

                    productsList.MyList.Add(newProduct);

                }
            });


            //NON ASYNC

            //foreach (var urlAdress in urlList)
            //{
            //    var httpClient = new HttpClient();
            //    var html = await httpClient.GetStringAsync(urlAdress);

            //    var htmlDocument = new HtmlDocument();
            //    htmlDocument.LoadHtml(html);


            //    var ProductList = htmlDocument.DocumentNode.Descendants(0)
            //        .Where(n => n.HasClass("offer")).ToList();

            //    var sb = new StringBuilder();

            //    var test = htmlDocument.DocumentNode.Descendants("link")
            //                .Select(node => node.GetAttributeValue("href", ""))
            //                .FirstOrDefault();

            //    if (!test.Contains("search"))
            //    {
            //        break;
            //    }

            //    foreach (var ProductListItem in ProductList)
            //    {

            //        string title;
            //        string price;
            //        string link;
            //        string image;

            //        try
            //        {
            //            title = ProductListItem.Descendants("a")
            //                .Where(node => node.GetAttributeValue("data-cy", "")
            //                .Equals("listing-ad-title")).FirstOrDefault()
            //                .InnerHtml.Trim('\r', '\t', '\n');

            //            price = ProductListItem.Descendants("p")
            //                .Where(node => node.GetAttributeValue("class", "")
            //                .Equals("price")).FirstOrDefault()
            //                .InnerHtml.Trim('\r', '\t', '\n');

            //            link = ProductListItem.Descendants("a")
            //                .Select(node => node.GetAttributeValue("href", ""))
            //                .FirstOrDefault().Trim();

            //            image = ProductListItem.Descendants("img")
            //                .Select(node => node.GetAttributeValue("src", ""))
            //                .FirstOrDefault().Trim();
            //        }
            //        catch (Exception)
            //        {
            //            continue;
            //        }

            //        var newProduct = new Product()
            //        {
            //            Title = title.ToString().Trim('\r', '\t', '\n').Replace("<strong>", "").Replace("</strong>", "").Replace("&quot;", ""),
            //            Price = price.ToString().Trim('\r', '\t', '\n').Replace("<strong>", "").Replace("</strong>", ""),
            //            Link = link.ToString().Trim('\r', '\t', '\n'),
            //            Image = image.ToString().Trim('\r', '\t', '\n')
            //        };
            //        if (result.MyList.Any(x => x.Title == newProduct.Title))
            //            continue;
            //        result.MyList.Add(newProduct);

            //    }
            //}


            //TODO: Order by price or search engine
            productsList.MyList = productsList.MyList.OrderBy(p => p.Price).ToList();


            return productsList;
        }
    }
}