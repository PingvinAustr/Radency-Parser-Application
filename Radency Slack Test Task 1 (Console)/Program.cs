using System;
using System.IO;
using System.Collections;
using System.Text;
using System.Text.RegularExpressions;
using Radency_Slack_Test_Task_1__Console_.Classes;
using System.Text.Json;


namespace Radency_Slack_Test_Task_1__Console_ 
{
    internal class Program
    {

        //Змінні для подальшого занесення в meta.log
        static int num_of_parsed_files = 0;
        static int num_of_parsed_lines = 0;
        static int lines_with_errors = 0;
        static List<FileInfo> invalid_files = new List<FileInfo>();
        static int num_of_conv_file = 1;
        static List<string> invalid_lines = new List<string>();

        /*
         Гайд для розуміння програми:
         1. Виконання починається з виклику ShowMenu() з Main(), де юзер обирає бажану опцію (запустити сканування/припинити виконання програми) 
         2. Якщо юзер обрав "Запустити сканування" - спочатку запускається функція CheckCreateDirectories(), яка перевіряє наявність та валідність конфігу й якщо він валіден - повертає шлях до input&output папок
         3. Запускається функція ReadFilesFromInputFolder(), яка повертає всі валідні (.txt та .csv) файли з вхідної директорії
         4. Запускається функція ReadInfoFromFilesAndValidate(), яка по черзі проходиться по всім input  файлам й запускає для них функцію ConvertFileToList(). Також після цього поточна функція створює лог файл
         5. В функції ConvertFileToList() за допомогою регулярних виразів валідується кожен атрибут поточного рядку файлу й якщо рядок валіден - запам'ятовує його наприкінці перетворює на json        
         Більш детальні коментарі написані в самому коді
         
         */




        static void ShowMenu()
        {
            while (true)
            {
                Console.WriteLine("Radency Slack Test Task 1 - ETL\n1.Start\n2.Stop(exit)\nPlease enter 1 or 2 to select action:");
                string input_choice;
                input_choice = Console.ReadLine();

                if (input_choice!="1" && input_choice != "2")
                {
                    Console.Clear();
                    Console.WriteLine("Please enter valid number:");
                }
                else
                {
                    switch (input_choice)
                    {
                        case "1":
                            {

                                Console.Clear();
                                num_of_parsed_files = 0;
                                num_of_parsed_lines= 0;
                                invalid_lines.Clear();                              
                                num_of_conv_file = 1;

                                //Отримуємо інпут+аутпут директорії
                                Tuple<string, string> tuple = CheckCreateDirectories();
                                string input_file = tuple.Item1;
                                string output_file = tuple.Item2;

                                //Читаємо, валідуємо, зберігаємо json
                                ReadInfoFromFilesAndValidate(ReadFilesFromInputFolder(input_file, output_file),output_file);

                                Console.WriteLine("\nPress any key to continue...");
                                Console.ReadKey();
                                break;
                            }
                        case "2":
                            {
                                Console.Clear();
                                Console.WriteLine("Thank you for using this application. See you later!");
                                Environment.Exit(0);
                                break;
                            }

                    }
                }

            }
        }

        //Перевіряємо чи валіден конфіг й зчитуємо з нього input & output директорії
        static Tuple<string,string> CheckCreateDirectories()
        {
            string input_folder = "", output_folder = "";
            string path_to_config = @"..\..\..\ProjectData\config.txt";
            

            //Якщо немає конфігу - програма не працює
           if (!File.Exists(path_to_config))
            {
                Console.Clear();
                Console.WriteLine("Config file is missing! Please add config file to ../ProjectData/config.txt");
                Environment.Exit(0);
            }

           //Якщо конфіг є - перевіряємо його валідність 
            else
            {
                string[] config_settings=File.ReadAllLines(path_to_config);
                bool is_config_valid = false;
                int is_config_valid_counter = 0;
                string regex_input = @"input_data_folder_name:'\S+'";
                string regex_output = @"output_data_folder_name:'\S+'";

               
                //Перевіряємо валідність й отримуємо потрібні директорії
                foreach (string item in config_settings)
                {
                    if (Regex.IsMatch(item, regex_input) || Regex.IsMatch(item, regex_output)) is_config_valid_counter++;            
                    if(item.Contains("input_data_folder_name")) { input_folder = item.Substring(item.IndexOf("'")+1, item.Length - item.IndexOf("'") - 2); }
                    else if (item.Contains("output_data_folder_name")) { output_folder = item.Substring(item.IndexOf("'") + 1, item.Length - item.IndexOf("'") - 2); }
                }
               

                //Додаткова валідація
                if (is_config_valid_counter == config_settings.Length)
                {
                   if (output_folder == input_folder)
                    {
                        Console.Clear();
                        Console.WriteLine("Input&output folders can't be the same! Please change config.txt and try again");
                        Environment.Exit(0);
                    }
                    else
                    {   //Якщо конфіг валіден - створюємо відповідні директорії
                        Directory.CreateDirectory(@"../../../ProjectData/" + input_folder);
                        Directory.CreateDirectory(@"../../../ProjectData/" + output_folder);
                    }                 
                }
                else                
                {
                    Console.Clear();
                    Console.WriteLine("Config is not valid! Please make sure that your config corresponds app rules:\n1.Config syntax should be like %setting name%+':'+'+%setting value%+'.\n2.All settings should be written from the new line.\n3.No empty lines allowed.\n4.Input/output folders should be specified with parameters input_data_folder_name & output_data_folder_name:'folder_b'.\n5.All pathes to files are considered to begin from internal 'ProjectData' folder. So if you set output_data_folder_name:'folder_b' - it means path 'ProjectData\folder_b', feel free to add as many subfolders to the path as you wish.\n6. Input and output folders can't be the same!");
                    Environment.Exit(0);
                }
                
            }
            return Tuple.Create(input_folder, output_folder);
          
        }

        //Отримуємо перелік валідних файлів для читання
        static List<FileInfo> ReadFilesFromInputFolder(string input_folder,string output_folder) {
          
            
            DirectoryInfo input_dir=new DirectoryInfo(@"../../../ProjectData/"+input_folder);
            List<FileInfo> input_files = new List<FileInfo>();
                  
            input_files=input_dir.GetFiles().ToList();
            
            //Всі !txt && !csv файли - не валідні 
            invalid_files = input_files.Where(x => x.Name.IndexOf(".txt") == -1 && x.Name.IndexOf(".csv") == -1).ToList();
            num_of_parsed_files+=invalid_files.Count;

            //txt && csv файли - валідні
            input_files = input_files.Where(x => x.Name.IndexOf(".txt") != -1 || x.Name.IndexOf(".csv")!=-1).ToList();                            
            return input_files;
        }

        //Читаємо вміст файлів та валідуємо їх
        static void ReadInfoFromFilesAndValidate(List<FileInfo> input_files,string output_file)
        {
           
            //Йдемо по кожному з валідних файлів
            foreach (FileInfo input_file in input_files)
            {
                
                Console.WriteLine("___");

                //Перелік валідних рядків(платежів) з поточного файлу
                List<SinglePayment> ValidPaymentsFromCurrentFile = new List<SinglePayment>();
                

                //Якщо це - txt файл
                if (input_file.Name.IndexOf(".txt") != -1 && input_file.Name.IndexOf(".csv") == -1)
                {
                    //Запускаємо процедуру перетворення валідних рядків поточного файлу на JSON
                    ConvertFileToList(input_file,".txt",ref ValidPaymentsFromCurrentFile, output_file);                   

                }

                //Якщо це - csv файл
                else if (input_file.Name.IndexOf(".txt") == -1 && input_file.Name.IndexOf(".csv") != -1)
                {
                    //Запускаємо процедуру перетворення валідних рядків поточного файлу на JSON
                    ConvertFileToList(input_file, ".csv", ref ValidPaymentsFromCurrentFile,output_file);                    
             }
            }

            
            string date_folder = DateTime.Now.ToString("MM-dd-yyyy");

            //Якщо  немає створеного meta.log - створюємо та наповнюємо його
            if (!System.IO.File.Exists(@"../../../ProjectData/" + output_file + "/" + date_folder + "/meta.log"))
            {
                string files_to_log = "";
                foreach (var item in invalid_files)
                {
                    files_to_log += item.FullName + ",";                   
                }
                files_to_log=files_to_log.Remove(files_to_log.Length-1);
                string log = "parsed_files:"+num_of_parsed_files+"\nparsed_lines:"+num_of_parsed_lines+"\nfound_errors:"+invalid_lines.Count()+"\ninvalid_files:"+files_to_log;
                
                System.IO.File.AppendAllText(@"../../../ProjectData/" + output_file + "/" + date_folder + "/meta.log", log);
            }
            
            //Якщо є створений meta.log - отримуємо його поточні дані й додаємо до них нові
            else
            {
                int history_parsed_files=0, history_parsed_lines=0, history_error_lines=0;
                string history_error_files = "";
                FileInfo file = new FileInfo(@"../../../ProjectData/" + output_file + "/" + date_folder + "/meta.log");
                using (StreamReader reader = file.OpenText())
                {
                    string str = "";
                    int row=0;
                    while ((str = reader.ReadLine()) != null)
                    {
                        switch (row)
                        {
                            case 0:
                                {
                                    history_parsed_files=int.Parse(str.Substring(str.IndexOf(":")+1, str.Length-str.IndexOf(":")-1));                               
                                    break;
                                }
                            case 1:
                                {
                                    history_parsed_lines=int.Parse(str.Substring(str.IndexOf(":") + 1, str.Length - str.IndexOf(":") - 1));
                                    break;
                                }
                            case 2:
                                {
                                    history_error_lines= int.Parse(str.Substring(str.IndexOf(":") + 1, str.Length - str.IndexOf(":") - 1));
                                    break;
                                }
                            case 3:
                                {

                                    foreach (var item in invalid_files)
                                    {
                                        history_error_files = str.Remove(0,14);
                                        if (str.IndexOf(item.FullName) == -1)
                                        {
                                            history_error_files += "," + item.FullName;
                                        }
                                    }
                                    break;
                                }
                        }
                        row++;
                    }
                }


                string log = "parsed_files:" +(history_parsed_files+num_of_parsed_files) + "\nparsed_lines:" +(history_parsed_lines+num_of_parsed_lines) + "\nfound_errors:" + (history_error_lines+invalid_lines.Count()) + "\ninvalid_files:" + history_error_files;
                System.IO.File.WriteAllText(@"../../../ProjectData/" + output_file + "/" + date_folder + "/meta.log", log);
            }

        }


       //Перетворюємо валідні рядки поточного файлу на json
        static void ConvertFileToList(FileInfo input_file,string type, ref List<SinglePayment> ValidPaymentsFromCurrentFile,string output_file)
        {
            
                        int invalid_lines_in_file = 0;
                        using (StreamReader reader = input_file.OpenText())
                        {
                             string str = "";
                             int step = 0;

                            //Йдемо кожним рядком поточного файлу
                            while ((str = reader.ReadLine()) != null)
                            {
                                //Допоміжні змінні
                                 num_of_parsed_lines++;
                                 step++;                               
                                 str += ",";

                                //Отримуємо параметри поточного рядку що розділені комами. Згідно умови коми є завжди, тобто для визначення параметрів можна від них відштовхуватись
                                 List<string> current_line_parameters = str.Split(',').ToList();

                                //Кількість пустих параметрів(неправильно розбиті дані)
                                int number_of_invalid_cells = current_line_parameters.Where(x => x.Length == 0).Count();

                                //Якщо поточний файл-csv та це перший рядок - пропускаємо (це хедер)
                                 if (type == ".csv" && step == 1) continue;

                                 //Початкова валідація рядку
                                else if (current_line_parameters.Count() != 10 || number_of_invalid_cells != 1)
                                {
                                    invalid_lines.Add(str);
                                    invalid_lines_in_file++;                               
                                    continue;
                                }                               

                                else
                                {
                                    //Створюємо об'єкт-платіж
                                    SinglePayment payment = new SinglePayment();
                                    int check_valid_params = 0;

                                    //Йдемо по кожному параметру
                                    for (int i = 0; i < current_line_parameters.Count(); i++)
                                    {
                                        //Отримуємо поточний параметер
                                        string current_parameter = current_line_parameters[i].Trim();
                                        
                                        //Нижче за допомогою регулярних виразів та логіки перевіряємо кожен параметр (наприклад, назва міста завжди в великої літери, в нії не може бути цифр й тд) 

                                        //Name
                                        if (i == 0)
                                        {
                                            string name_regex = @"^[a-zA-Z]+$";
                                            if (!Regex.IsMatch(current_parameter, name_regex))
                                            {
                                                invalid_lines.Add(str);
                                                invalid_lines_in_file++;                                         
                                                break;
                                            }
                                            else
                                            {
                                                payment.first_name = current_parameter;
                                                check_valid_params++;
                                            }
                                        }

                                        //Surname
                                        else if (i == 1)
                                        {
                                            string name_regex = @"^[a-zA-Z]+$";
                                            if (!Regex.IsMatch(current_parameter, name_regex))
                                            {
                                                invalid_lines_in_file++;
                                                invalid_lines.Add(str);
                                                break;
                                            }
                                            else
                                            {
                                                payment.last_name = current_parameter;
                                                check_valid_params++;
                                            }
                                        }

                                        //City
                                        else if (i == 2)
                                        {
                                            current_parameter = current_parameter.Substring(1, current_parameter.Length - 1);

                                            string city_regex = @"^[A-Z][a-z]+$";
                                            if (!Regex.IsMatch(current_parameter, city_regex))
                                            {
                                                invalid_lines_in_file++;
                                                invalid_lines.Add(str);
                                                break;
                                            }
                                            else
                                            {
                                                payment.city = current_parameter;
                                                check_valid_params++;
                                            }
                                        }

                                        //Street
                                        else if (i == 3)
                                        {
                                            string street_regex = @"^[A-Z][a-z]+ [0-9]+$";
                                            if (!Regex.IsMatch(current_parameter, street_regex))
                                            {
                                                invalid_lines_in_file++;
                                                invalid_lines.Add(str);
                                                break;
                                            }
                                            else
                                            {
                                                payment.address = current_parameter;
                                                check_valid_params++;
                                            }
                                        }

                                        //Flat
                                        else if (i == 4)
                                        {
                                            current_parameter = current_parameter.Substring(0, current_parameter.Length - 1);
                                            try
                                            {
                                                int.Parse(current_parameter);
                                                check_valid_params++;
                                            }
                                            catch
                                            {
                                                invalid_lines_in_file++;
                                                invalid_lines.Add(str);
                                                break;
                                            }
                                        }

                                        //Payment
                                        else if (i == 5)
                                        {
                                            try
                                            {
                                                payment.payment = decimal.Parse(current_parameter.Replace(".", ","));
                                                check_valid_params++;
                                            }
                                            catch
                                            {
                                                invalid_lines_in_file++;
                                                invalid_lines.Add(str);
                                                break;
                                            }
                                        }

                                        //Date
                                        else if (i == 6)
                                        {
                                            try
                                            {
                                                payment.date = DateTime.ParseExact(current_parameter, "yyyy-dd-MM", null);
                                                check_valid_params++;
                                            }
                                            catch
                                            {
                                                invalid_lines_in_file++;
                                                invalid_lines.Add(str);
                                                break;
                                            }
                                        }

                                        //Account
                                        else if (i == 7)
                                        {
                                            try
                                            {
                                                payment.account_name = long.Parse(current_parameter);
                                                check_valid_params++;
                                            }
                                            catch
                                            {
                                                invalid_lines_in_file++;
                                                invalid_lines.Add(str);
                                                break;
                                            }
                                        }

                                        else if (i == 8)
                                        {
                                            string service_regex = @"^[A-Z][a-z]+$";

                                            if (Regex.IsMatch(current_parameter, service_regex))
                                            {
                                                payment.service = current_parameter;
                                                check_valid_params++;
                                            }
                                            else
                                            {
                                                invalid_lines_in_file++;
                                                invalid_lines.Add(str);
                                                break;
                                            }
                                        }

                                    }

                                    if (check_valid_params == 9)
                                    {
                                        //Якщо вдалось пройти всі етапи валідації - додаємо поточний платіж до списку всіх валідних платежів
                                        ValidPaymentsFromCurrentFile.Add(payment);
                                    }
                                }
                            }
                        }
                        //Виводимо повідомлення про парсінг поточного файлу та його результати
                        Console.WriteLine("File " + input_file.Name + " parsed. Invalid lines in this file - " + invalid_lines_in_file + ". Successfully parsed - " + ValidPaymentsFromCurrentFile.Count() + " lines");

                        //Нижче починається перетворення даних зі списку ValidPaymentsFromCurrentFile на json. Як саме?
                        //Ми маємо декілька файлів - SinglePayment, Services_, Payers, Cities 
                        //SinglePayment - один рядок(платіж) файлу
                        //Payers - клас платників 
                        //Services_ - клас сервісу, який має в собі список платників
                        //Cities - клас міста, який має в собі список сервісів

                        num_of_parsed_files++;

                        //Список унікальних сервісів
                        List<Services_> services_list = new List<Services_>();

                        //Список унікальних міст
                        List<Cities> cities_list = new List<Cities>();

                        //Йдемо по кожному рядку щоб отримати перелік унікальних сервісів та міст з поточного файлу
                        foreach (SinglePayment singlePayment in ValidPaymentsFromCurrentFile)
                        {
                            Services_ services = new Services_();
                            services.service = singlePayment.service;
                            if (services_list.Where(x => x.service == services.service).Count() == 0)
                            {
                                services_list.Add(services);
                            }

                            Cities cities = new Cities();
                            cities.city = singlePayment.city;
                            if (cities_list.Where(x => x.city == cities.city).Count() == 0)
                            {
                                cities_list.Add(cities);
                            }


                        }


                        //Вище ми отримали перелік всіх сервісів та міст з цього файлу

                        //Створюємо список, який далі і стане json-файлом-відповіддю
                        List<Cities> CitiesList = new List<Cities>();

                        //Йдемо по кожному місту
                        foreach (var city in cities_list)
                        {
                            //Створюємо список та отримуємо перелік всіх платежів з цього міста
                            List<SinglePayment> PaymentsFromThisCity = new List<SinglePayment>();
                            PaymentsFromThisCity = ValidPaymentsFromCurrentFile.Where(x => x.city == city.city).ToList();
                            
                            //Стоврємо об'єкт поточного міста (яке розглядається на поточнії ітерації foreach)
                            Cities City = new Cities();
                            City.city = city.city;

                            //Ініціалізуємо список сервісів міста (поки що він пустий)
                            List<Services_> services_per_city = new List<Services_>();

                            //Ініціалізуємо суму платежів по місту
                            decimal city_total = 0;

                            //Йдемо по кожному сервісу взагалі
                            foreach (var service in services_list)
                            {
                                //Створюємо об'єкт поточного сервісу (який розглядається на поточнії ітерації foreach)
                                Services_ ServiceS = new Services_();
                                ServiceS.service = service.service;

                                //На основі списку PaymentsFromThisCity (список платежів з цього міста) отримую список платежів з цього міста та за поточним сервісом
                                List<SinglePayment> PaymentsFromThisCityByService = new List<SinglePayment>();
                                PaymentsFromThisCityByService = PaymentsFromThisCity.Where(x => x.service == service.service).ToList();


                                //Ініціалізуємо список платників за поточним містом та поточним сервісом (поки що він пустий)
                                List<Payers> payers_per_service_per_city = new List<Payers>();

                                //Ініціалізуємо суму платежів по місту по сервісу
                                decimal service_total = 0;

                                //Йдемо по всім платежам цього міста цього сервісу
                                foreach (var item in PaymentsFromThisCityByService)
                                {
                                    //Додаємо суму платежу в загальну суму поточного сервісу
                                    service_total += item.payment; 

                                    //Створюємо об'єкт платника та додаємо його в список платників поточного сервісу поточного міста
                                    Payers payer = new Payers() { account_name = item.account_name, date = item.date.ToString("yyyy-dd-MM"), name = item.first_name+" "+item.last_name, payment = item.payment };
                                    payers_per_service_per_city.Add(payer);
                                }

                                //До раніше створеного об'єкту поточного сервісу додаємо отриману суму платежів та список платників цього сервісу в цьому місті
                                ServiceS.total = service_total;
                                ServiceS.payers = payers_per_service_per_city;

                                //Додаємо поточний сервіс в список сервісів міста
                                services_per_city.Add(ServiceS);

                                //Додаємо суму платежів поточного сервіса до суми платежів поточного міста
                                city_total += service_total;
                            }

                            //Запам'ятовуємо отриману суму платежів за поточним містом та список платежів за сервісами
                            City.total = city_total;
                            City.services = services_per_city;
                            CitiesList.Add(City);
                        }
                        //Після виконання вищенаведеного коду отримуємо список CitiesList, що за структурою є потрібним JSON

                        //Перетворюємо список на JSON
                        string json = JsonSerializer.Serialize(CitiesList);
                        
                        //Отримуємо назву поточної папки для збереження
                        string date_folder=DateTime.Now.ToString("MM-dd-yyyy");
                        Directory.CreateDirectory(@"../../../ProjectData/" + output_file+"/"+date_folder);
            
                        //Якщо це не повторний парсинг - зберігаємо цей файл
                        if (!System.IO.File.Exists("../../../ProjectData/" + output_file + "/" + date_folder + "/output" + num_of_conv_file + ".json"))
                        System.IO.File.AppendAllText("../../../ProjectData/"+output_file + "/"+date_folder+"/output"+num_of_conv_file+".json", json);          
                        num_of_conv_file++;
           
        
        }

        static void Main(string[] args)
        {
            ShowMenu();     
        }
    }
}