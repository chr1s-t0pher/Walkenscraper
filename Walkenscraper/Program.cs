using Solnet.Rpc;
using Solnet.Extensions;
using Solnet.Wallet;
using Solana.Metaplex;
using Walkenscraper;
using System.Net.Http.Json;
using HtmlAgilityPack;
using PuppeteerSharp;


try
{
    //get decimal seperator
    char[] seperator = System.Globalization.CultureInfo.CurrentUICulture.NumberFormat.NumberDecimalSeparator.ToCharArray();
    //string csv_seperator = ";";

    // load Solana token list and get RPC client
    var client = ClientFactory.GetClient("https://solana-api.projectserum.com");
    var tokens = TokenMintResolver.Load();

    //build data list to be written to cathletes.csv
    var data = new List<string>();
    data.Add("Nickname;Level;Rarity;breed;max. breed;gems;gems to next lvl;nude Lvl. 10 strength;dressed Lvl. 10 strength;current Strength;nude Lvl. 10 stamina;dressed Lvl. 10 stamina;current Stamina;nude Lvl. 10 speed;dressed Lvl. 10 speed;current Speed;Color Rarity;Color Rarity Percentage;Enviroment Rarity;Enviroment Rarity Percentage;Body Rarity;Body Rarity Percentage;Tail Rarity;Tail Rarity Percentage;Ears Rarity;Ears Rarity Percentage;Face Rarity;Face Rarity Percentage;Image;External URL;");

    //Ask for Public Solana key
    Console.WriteLine("Public Key?");
    var publicKey = Console.ReadLine();
    Console.WriteLine();

    //load wallets
    var tokenWallet = TokenWallet.Load(client, tokens, publicKey);

    foreach (var account in tokenWallet.TokenAccounts())
    {
        //check if is nft?
        if (account.DecimalPlaces == 0 && account.QuantityDecimal == 1)
        {
            //get metadata
            var Metadata = await MetadataAccount.GetAccount(client, new PublicKey(account.PublicKey), 3);

            //check if is Walken Cathlete
            if (Metadata.metadataV3.symbol.Equals("WLKNC"))
            {

                //desirilize Json file
                using HttpClient httpclient = new()
                {
                    BaseAddress = new Uri(Metadata.metadataV3.uri)
                };
                WLKNC? wlknc = await httpclient.GetFromJsonAsync<WLKNC>("");

                Console.WriteLine("Reading values: " + wlknc.attributes[0].value.ToString());
                //use puppeteer to open url and wait till all scripts are loaded
                using var browserFetcher = new BrowserFetcher();
                await browserFetcher.DownloadAsync(BrowserFetcher.DefaultChromiumRevision);
                var browser = await Puppeteer.LaunchAsync(new LaunchOptions
                {
                    Headless = true

                });
                var page = await browser.NewPageAsync();
                await page.GoToAsync(wlknc.external_url, WaitUntilNavigation.Networkidle0);

                var content = await page.GetContentAsync();
                await browser.CloseAsync();
                //use HtmlAgilityPAck to search in content Respone
                HtmlAgilityPack.HtmlDocument DocToParse = new HtmlAgilityPack.HtmlDocument();
                DocToParse.LoadHtml(content);
                //get current and max. Strenght, Stamina, and Speed
                int i = 0;
                string[] Strength = { "", "" }, Stamina = { "", "" }, Speed = { "", "" };
                foreach (HtmlNode node in DocToParse.DocumentNode.SelectNodes("//div[@class=\"StatisticItem__Range-sc-1a896ok-5 dAzdSe\"]"))
                {



                    if (i == 0)
                        Strength = node.InnerText.Split('/');
                    if (i == 1)
                        Stamina = node.InnerText.Split('/');
                    if (i == 2)
                    {
                        Speed = node.InnerText.Split('/');
                        break;
                    }


                    i++;
                }

                //get current level
                var lvl = DocToParse.DocumentNode.SelectSingleNode("//span[@class=\"CathleteLevel__Level-sc-v64rqk-3 hWlTYV\"]");


                //get current breedcount and gems
                i = 0;
                string[] breed = { "", "" };
                string[] gems = { "", "" };
                foreach (HtmlNode node in DocToParse.DocumentNode.SelectNodes("//div[@class=\"Specificationsstyles__BadgeGroupValues-sc-r8nabu-7 gArjoB\"]"))
                {
                    if (i == 0)
                    {
                        gems = node.InnerText.Split('/');
                    }
                    if (i == 1)
                    {
                        breed = node.InnerText.Split('/');
                        break;
                    }
                    i++;
                }
                //get rarity levels for body parts       
                i = 0;
                string color_rar = "", enviroment_rar = "", body_rar = "", tail_rar = "", ears_rar = "", face_rar = "";
                foreach (HtmlNode node in DocToParse.DocumentNode.SelectNodes("//span[@class=\"Raritystyles__Text-sc-bi8l6g-2 gfzpHQ\"]"))
                {
                    if (i == 0)
                        color_rar = node.InnerText;
                    if (i == 1)
                        enviroment_rar = node.InnerText;
                    if (i == 2)
                        body_rar = node.InnerText;
                    if (i == 3)
                        tail_rar = node.InnerText;
                    if (i == 4)
                        ears_rar = node.InnerText;
                    if (i == 5)
                    {
                        face_rar = node.InnerText;
                        break;
                    }
                    i++;

                }
                //get rarity percentage levels for body parts
                string color_p = "", enviroment_p = "", body_p = "", tail_p = "", ears_p = "", face_p = "";
                i = 0;
                foreach (HtmlNode node in DocToParse.DocumentNode.SelectNodes("//span[@class=\"RarityItem__Text-sc-bbwv87-4 bBWuGz\"]"))
                {

                    if (i == 0)
                        color_p = node.InnerText.Substring(0, node.InnerText.IndexOf('%'));
                    if (i == 1)
                        enviroment_p = node.InnerText.Substring(0, node.InnerText.IndexOf('%'));
                    if (i == 2)
                        body_p = node.InnerText.Substring(0, node.InnerText.IndexOf('%'));
                    if (i == 3)
                        tail_p = node.InnerText.Substring(0, node.InnerText.IndexOf('%'));
                    if (i == 4)
                        ears_p = node.InnerText.Substring(0, node.InnerText.IndexOf('%'));
                    if (i == 5)
                    {
                        face_p = node.InnerText.Substring(0, node.InnerText.IndexOf('%'));
                        break;
                    }
                    i++;

                }
                

                //add to data
                data.Add(wlknc.attributes[0].value.ToString() + ";" + lvl.InnerText.Remove(0, 5) + ";" + wlknc.attributes[1].value.ToString() + ";" + breed[0] + ";" + breed[1] + ";" + gems[0] + ";" + gems[1] + ";" +
                wlknc.attributes[2].value.ToString() + ";" + Strength[1].Replace('.', seperator[0]) + ";" + Strength[0].Replace('.', seperator[0]) + ";" + wlknc.attributes[3].value.ToString() + ";" + Stamina[1].Replace('.', seperator[0]) + ";" + Stamina[0].Replace('.', seperator[0]) + ";" +
                wlknc.attributes[4].value.ToString() + ";" + Speed[1].Replace('.', seperator[0]) + ";" + Speed[0].Replace('.', seperator[0]) + ";" +
                color_rar + ";" + color_p.Replace('.', seperator[0]) + ";" + enviroment_rar + ";" + enviroment_p.Replace('.', seperator[0]) + ";" + body_rar + ";" + body_p.Replace('.', seperator[0]) + ";" + tail_rar + ";" + tail_p.Replace('.', seperator[0]) + ";" + ears_rar + ";" + ears_p.Replace('.', seperator[0]) + ";" + face_rar + ";" + face_p.Replace('.', seperator[0]) + ";" +
                wlknc.image + ";" + wlknc.external_url);


                //dont ask to many cathletes so wait for 6s
                Console.WriteLine("Wait 10s for next");
                Thread.Sleep(10000);
                //wlknc.attributes[1].value.ToString() rarity
                //lvl.InnerText.Remove(0, 5) level


            }

            /*
            if (Metadata.metadataV3.symbol.Equals("WLKNCI"))        
            {
            maybe added later

            }*/


        }


    }
    //write to file
    System.IO.File.WriteAllLines(@"CATthletes-"+publicKey+ ".csv", data);
    Console.WriteLine();
    Console.WriteLine("finished");
    Console.ReadKey();
}
catch (Exception e)
{
    Console.WriteLine(e.Message);
    Console.ReadKey();
}


