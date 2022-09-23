# C# Console Application
## Idea of the application:
User has several files (.txt / .csv) with data about communal payments of people (Name/Surname/City/Address/PaymentSum/Date/Account/Type(Water, Gas, etc). 
Input file can contain hundreds/thousands of data rows.
Data from input files is layered (City-->Payments-->People). Application parsers input data and returns 1 JSON file for each input file. JSON file has data grouped by city and by payment.
### Example of input file content (txt):
![alt text](https://media.discordapp.net/attachments/627965989899993138/1022840279331512350/unknown.png)
### Example of input file content (csv): 
![alt text](https://media.discordapp.net/attachments/627965989899993138/1022845509343395880/unknown.png?width=463&height=676)

Project has config file that is located in ProjectDirectory/ProjectData/config.txt
Application will not function without config file.
Config file content:
![alt text](https://media.discordapp.net/attachments/627965989899993138/1022841107333255178/unknown.png)
User is able to set folder for input files and for output files.

Application has console interface.

### Application validates everything:
* Config file (names of parameters should be exacty like on the screenshot
* Input files should be only CSV/TXT
* All parameters (Name/City/Address/etc) are validated as well (Name cannot contain numbers or symbols, etc)

### Application parses each file and saves number of invalid/valid lines:
![alt text](https://media.discordapp.net/attachments/627965989899993138/1022843002009440326/unknown.png)

### Output directory structure (output files are saved to directory with current date):
![alt text](https://media.discordapp.net/attachments/627965989899993138/1022843312962543676/unknown.png)
![alt text](https://media.discordapp.net/attachments/627965989899993138/1022844177198895145/unknown.png)
#### Output directory for current day also has meta.log file, which stores log about all parsed files: 
![alt text](https://media.discordapp.net/attachments/627965989899993138/1022844416169353236/unknown.png?width=1440&height=118)

#### The most important part - example of output JSON file:
![alt text](https://media.discordapp.net/attachments/627965989899993138/1022844924892282940/unknown.png?width=1440&height=586)
So as shown - all payments are grouped by cities and by service type (gas/heat/etc)

### Notes:
* LINQ is used where possbile
* Idea of speeding convertation process up - multithreading. 
