# CoffeeShopAPI

This application make use ASP.Net Core Web API to create a library of RESTful endpoints that allow users to access information about coffee beans. I decided to choose ASP.Net Core as it has been a framework that I have always enjoyed using and developing in and for the past 3 years this is something I have been unable  do.

This application tries to follow the SOLID principles with each function in the application performing one role. The advantages of this is that it makes the codebase easy to read, test and debug well as potentially making it easy to refactor at a later date.

This application also does not make use of any stored procedures or batch jobs. Both could have easily be chosen, however I have decided to try and keep as much of the logic in this application inside the codebase. This means that for database interactions I am using EF and using a background service to select the 'Bean of the Day'.

The API utilises Basic Auth and so users will not be able to call and read from functions without entering the correct credentials (Found below):
- dev@core.dev
- dev

Although I have provided a database dump. The application can still be loaded with initial data by utilising the LoadInitialData endpoint. This will read information from a json file located within the codebase and input that data into the database.

Testing
- I believe the most 'complex' part of the application is the background service which is utilised to select the botd. Becuase of this, I have implemented tests check for eventualities such as if there are no beans as well as how Universal Time would affect how the 'calculate' function would behavour.
- We also have a test suit to test the CoffeeBeanRepository. Although the functionality of this is not as complex, it is still vital in the functionality of the API.
