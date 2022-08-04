using System;
using System.IO;
using System.Text.RegularExpressions;

namespace Radency_Slack_Test_Task_1__Console_ // Note: actual namespace depends on the project name.
{
    internal class Program
    {


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
                                CheckCreateDirectories();
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

        static void CheckCreateDirectories()
        {
            string path_to_config = @"..\..\..\ProjectData\config.txt";
            
           if (!File.Exists(path_to_config))
            {
                Console.Clear();
                Console.WriteLine("Config file is missing! Please add config file to ../ProjectData/config.txt");
                Environment.Exit(0);
            }
            else
            {
                string[] config_settings=File.ReadAllLines(path_to_config);
                bool is_config_valid = false;
                int is_config_valid_counter = 0;
                string regex_input = @"input_data_folder_name:'\S+'";
                string regex_output = @"output_data_folder_name:'\S+'";

                string input_folder="", output_folder="";

                foreach (string item in config_settings)
                {
                    if (Regex.IsMatch(item, regex_input) || Regex.IsMatch(item, regex_output)) is_config_valid_counter++;            
                    if(item.Contains("input_data_folder_name")) { input_folder = item.Substring(item.IndexOf("'")+1, item.Length - item.IndexOf("'") - 2); }
                    else if (item.Contains("output_data_folder_name")) { output_folder = item.Substring(item.IndexOf("'") + 1, item.Length - item.IndexOf("'") - 2); }
                }
               
                if (is_config_valid_counter == config_settings.Length)
                {
                   if (output_folder == input_folder)
                    {
                        Console.Clear();
                        Console.WriteLine("Input&output folders can't be the same! Please change config.txt and try again");
                        Environment.Exit(0);
                    }
                    else
                    {
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
            //Console.WriteLine(Directory.GetCurrentDirectory());
        }


        static void Main(string[] args)
        {
            ShowMenu();
            
        }
    }
}