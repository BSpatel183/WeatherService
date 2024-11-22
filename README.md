# Weather Service API

This project is an ASP.NET Web API designed to provide weather information. The API interacts with external weather services to fetch real-time weather data for cities and countries. The API returns weather descriptions and corresponding icons, which can be consumed by frontend applications like the Weather Service UI.


## Features

- **Weather Data**: Fetch weather information (description, icon) for a specific city and country.
- **API Key Authentication**: Requires an API key for authentication.
- **Scalability**: Easily extendable to support additional endpoints or services.


## Tech Stack

- **Backend Framework**: ASP.NET Core Web API
- **HTTP Requests**: Uses RestSharp to interact with external weather services.
- **Authentication**: API key-based authentication.
- **Response Format**: JSON


## Installation

### Prerequisites
- .NET SDK 6.0 or higher
- Visual Studio or any IDE with .NET support

### Steps to Install

1. Clone the repository:
  ```
git clone https://github.com/BSpatel183/WeatherService.git
```

3. Navigate to the project directory:
```
cd WeatherService
```

5. Restore dependencies:
 ```
 dotnet restore
 ```
8. Build the project:
 ```
 dotnet build
 ```
10. Run the project:
 ```
dotnet run
```
11. Check the Swagger(Optional)
```
http://localhost:5034/swagger/index.html
```

## Step to Test Project
1. Navigate to the project directory:
```
cd WeatherService.tests
```
2. Build the project:
```
dotnet build
```
3. Run the test project:
```
dotnet test
```

## API Endpoints

### `/api/weather`

#### Method: `GET`

Fetches weather information based on the city and country parameters.

**Parameters:**
- `city`: Name of the city (string)
- `country`: Name of the country (string)
- `apiKey`: Your personal API key for authentication.

**Example Request:**
```http
GET http://localhost:5034/api/weather?city=Melbourne&country=australia&apikey=YourAPIKey
