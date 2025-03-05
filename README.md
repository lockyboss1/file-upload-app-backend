# File Upload App Backend

This project is a .NET 7-based API that uses Onion Architecture to process file uploads (CSV and Excel) for orders and stores them in MongoDB. The application validates the input data and provides endpoints to retrieve order information.

## Prerequisites

- [.NET 7 SDK](https://dotnet.microsoft.com/download/dotnet/7.0)
- [Docker](https://docs.docker.com/get-docker/) and [Docker Compose](https://docs.docker.com/compose/install/) installed on your machine.
- Ensure ports **8080** (for the API) and **27017** (for MongoDB) are free.

## Project Structure

- **Domain**: Contains domain entities and interfaces.
- **Application**: Contains business logic, services, DTOs, and interfaces.
- **Infrastructure**: Contains data context and repository implementations for MongoDB.
- **Presentation**: Contains API controllers.
- **Tests**: Contains unit tests for key components (CSV and Excel processing).
- **Dockerfile**: Multi-stage build file for the API.
- **docker-compose.yml**: Defines containers for the API and MongoDB.

## Setup & Running the Application

### Using Docker Compose

1. **Build and start the containers in detached mode:**
   ```sh
   docker-compose up --build -d 
   

2. **API is available at the following base URL:**
    ```http://localhost:8080


3. **Available Endpoints:**
    ```GET /api/orders - Retrieves all orders from the database.
    ```GET /api/orders/{orderNumber} - Retrieves a single order by its order number. i.e http://localhost:8080/api/orders/ORDER1

4. **Running Unit Tests:**
    ```1. Open a terminal in the solution root or the Tests project folder.Open a terminal in the solution root or the Tests project folder.
    2. Run the following command: dotnet test
 

5. **Stop and Remove the Containers:**
    ```When you need to shut down the application, run: docker-compose down     