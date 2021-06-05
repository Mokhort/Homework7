using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace Homework7_ryd
{
    class Program
    {
        public class SearchJson
        {
            public int place_id { get; set; }
            public string licence { get; set; }
            public string osm_type { get; set; }
            public int osm_id { get; set; }
            public List<string> boundingbox { get; set; }
            public string lat { get; set; }
            public string lon { get; set; }
            public string display_name { get; set; }
            public string @class { get; set; }
            public string type { get; set; }
            public double importance { get; set; }
            public string icon { get; set; }
        }

        async static Task Main(string[] args)
        {
            string name;
            string country;
            string city;

            Console.WriteLine("======REGISTRATION======:");
            Console.WriteLine("Your name:");
            name = Console.ReadLine();
            Console.WriteLine("Your country:");
            country = Console.ReadLine();
            Console.WriteLine("Your city:");
            city = Console.ReadLine();


            Context context = new Context();
            context.Database.EnsureCreated();

            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("User-Agent", "C# App");
                HttpResponseMessage pointsResponse = await client.GetAsync("https://nominatim.openstreetmap.org/search?country="+country+"&city="+city+"&format=json");

                if (pointsResponse.IsSuccessStatusCode)
                {
                    List<SearchJson> roots = await pointsResponse.Content.ReadFromJsonAsync<List<SearchJson>>();
                    foreach (SearchJson root in roots)
                    {
                        Console.WriteLine(root.display_name);
                    }
                    if (roots.Count() == 0) {
                        Console.WriteLine("City doesn't exist in this country");
                    }
                    else
                    {
                        IQueryable<Info> users = from user in context.Info
                                                 where (user.Name == name)
                                                 select user;
                        if (users.Count() != 0)
                        {
                            Console.WriteLine("User exists");
                        }
                        else
                        {
                            IDbContextTransaction transaction =
                                await context.Database.BeginTransactionAsync();

                            Info newUser = new Info()
                            {
                                Name = name,
                                Country = country,
                                City = city
                            };
                            context.Info.Add(newUser);
                            await transaction.CommitAsync();
                            await context.SaveChangesAsync();
                            Console.WriteLine("User was added succesfully");

                        }
                    }
                }
                else
                {
                    string resp = await pointsResponse.Content.ReadAsStringAsync();
                    Console.WriteLine(resp);
                }
            }

        }
    }
}
