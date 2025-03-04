# Pinterest-Like .NET Core MVC Application

## 📌 Description

This is a web application developed in **.NET Core MVC**, inspired by **Pinterest**, allowing users to save articles and images into personal collections. Users can organize content and interact with other collections.

---

## 📸 Screenshots

Home Page

![image](https://github.com/user-attachments/assets/9d68c5c4-4d09-4846-9893-1f7e66615073)

Registration Page

![image](https://github.com/user-attachments/assets/c3f155b1-2c54-4ec6-8e02-a0acb8714d98)

Registered Home Page - search and filter by likes/date functionalities

![image](https://github.com/user-attachments/assets/c7e27bb8-cecb-4a1b-a837-124e8a52d84f)

![image](https://github.com/user-attachments/assets/610266b1-25f4-4c46-b28d-236bbe2cf549)

User's Bookmarks Page

![image](https://github.com/user-attachments/assets/57cd383e-d6d3-40fc-9403-835519d7f405)


---

## ✨ Key Features

✅ Authentication and authorization (Identity Framework, JWT)&#x20;

✅ Create, edit, and delete collections&#x20;

✅ Add and organize articles&#x20;

✅ Search and filter content&#x20;

✅ Modern, responsive interface&#x20;

✅ REST API for integration with other applications - work in progress

---

## 🛠️ Technologies Used

- **.NET Core MVC**
- **Entity Framework Core** for database management
- **Identity Framework** for authentication and authorization
- **SQL Server**
- **Bootstrap CSS** for UI design
- **JavaScript & jQuery**
- **Docker**

---

## 🚀 Installation and Running the Application

1. Clone the repository:

   ```sh
   git clone https://github.com/Watergirll/Pinterest-2.0
   cd Pinterest-2.0
   ```

2. Install dependencies:

   ```sh
   dotnet restore
   ```

3. Configure the database connection in `appsettings.json`

4. Apply database migrations:

   ```sh
   dotnet ef database update
   ```

5. Run the application:

   ```sh
   dotnet run
   ```

---

## 🗄️ Database Structure&#x20;

ER Diagram

![image](https://github.com/user-attachments/assets/9b566fce-f7cd-40e7-a7b1-2f9aa35b2874)


Conceptual Diagram

![image](https://github.com/user-attachments/assets/3a8c1e9a-01d3-4f2d-b294-56cea0e67fac)

---

## 🤝 Contributions

Any contributions are welcome! Follow these steps:

1. **Fork** the repository
2. Create a **new branch** (`feature/feature-name`)
3. Make the necessary changes and **commit**
4. Submit a **Pull Request**

---

### 🔗 Useful Links

- [Official .NET Core Documentation](https://docs.microsoft.com/en-us/dotnet/core/)
- [Entity Framework Core](https://docs.microsoft.com/en-us/ef/core/)
- [Identity Framework](https://docs.microsoft.com/en-us/aspnet/core/security/authentication/identity)

