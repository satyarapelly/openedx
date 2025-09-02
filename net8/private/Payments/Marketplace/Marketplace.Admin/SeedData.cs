using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;
using ThirdParty.Model;


namespace Marketplace.Admin
{
    public static class SeedData
    {
        public static List<CatalogItem> GetBooks()
        {
            return new List<CatalogItem>()
            {
                new CatalogItem()
                {
                    Id = System.Guid.NewGuid().ToString(),
                    Product = new Product()
                    {
                        Title = "The Da Vince Code",
                        Subtitle = "Dan Brown - 2003",
                        Image = "/images/DaVinciCode.jpg",
                        Description = "While in Paris on business, Harvard symbologist Robert Langdon receives an urgent late-night phone call: the elderly curator of the Louvre has been murdered inside the museum. Near the body, police have found a baffling cipher. Solving the enigmatic riddle, Langdon is stunned to discover it leads to a trail of clues hidden in the works of da Vinci…clues visible for all to see…and yet ingeniously disguised by the painter.",
                        Seller = new Seller()
                        {
                            Name = "Fabrikam, Inc.",
                            Logo = "/images/fabrikam_logo.svg"
                        }
                    },
                    Skus = new List<ProductSku>()
                    {
                        new ProductSku()
                        {
                            Id = "001",
                            Price = 9.95d,
                            PaymentType = PaymentType.Immediate,
                            Title = "Download Now"
                        },
                        new ProductSku()
                        {
                            Id = "002",
                            Price = 16.95d,
                            PaymentType = PaymentType.Deferred,
                            Title = "Paperback"
                        }
                    }
                },
                new CatalogItem()
                {
                    Id = System.Guid.NewGuid().ToString(),
                    Product = new Product()
                    {
                        Title = "Hotel",
                        Subtitle = "Arthur Hailey - 2000",
                        Image = "/images/Hotel.jpg",
                        Description = "During five sultry days, the lives of the guests, the management, and the workers at New Orleans’ largest and most elite hotel converge. The owner has four days to raise the money to save his financially ailing property. The general manager, once blacklisted from the hospitality business, struggles with one crisis after another. A rebellious heiress will do anything to attain her secret desires. The duke and the duchess in the lavish presidential suite are covering up a crime. And within one of the many guest rooms hides a professional thief.",
                        Seller = new Seller()
                        {
                            Name = "Fabrikam, Inc.",
                            Logo = "/images/fabrikam_logo.svg"
                        }
                    },
                    Skus = new List<ProductSku>()
                    {
                        new ProductSku()
                        {
                            Id = "001",
                            Price = 9.99d,
                            PaymentType = PaymentType.Immediate,
                            Title = "Download Now"
                        },
                        new ProductSku()
                        {
                            Id = "002",
                            Price = 74.99d,
                            PaymentType = PaymentType.Deferred,
                            Title = "Hardcover"
                        }
                    }
                }
            };
        }

        public static List<CatalogItem> GetCatalogItems()
        {
            List<CatalogItem> retVal = null;
            using (StreamReader file = File.OpenText(@"SeedData\SeedData.Catalog.json"))
            {
                JsonSerializer serializer = new JsonSerializer();
                retVal = (List<CatalogItem>)serializer.Deserialize(file, typeof(List<CatalogItem>));
            }

            return retVal;
       }

        public static List<Seller> GetSellers()
        {
            List<Seller> retVal = null;
            using (StreamReader file = File.OpenText(@"SeedData\SeedData.Sellers.json"))
            {
                JsonSerializer serializer = new JsonSerializer();
                retVal = (List<Seller>)serializer.Deserialize(file, typeof(List<Seller>));
            }

            return retVal;
        }
    }
}
