# PostSnap 📸

PostSnap is a full-stack ASP.NET MVC web application that allows users to create, view, and interact with user-generated posts and comments. Designed with a clean UI and practical functionality, it's built to showcase core concepts like user authorization, role-based access, dynamic CRUD operations, and responsive UX with AJAX and jQuery.

## 🔍 Features

- **User Accounts with Identity**  
  Users can register with a username, email, and password. Authentication and role-based authorization are handled using ASP.NET Identity.

- **Post Management**
  - Users can create posts with a title, body, and an optional image.
  - Posts can be edited or soft-deleted by their owners.
  - Admins can hard-delete any post.

- **Comment System**
  - Users can add, edit, and soft-delete their own comments under posts.
  - Admins can fully manage all comments.
  - Admins can hard-delete any post.


- **AJAX-Driven Pagination**
  - Posts and comments are paginated with AJAX using `XPagedList`.
  - Some CRUD operations use jQuery for a smooth experience.

- **Image Uploads**
  - Preview selected image before upload.
  - Validates file type and max file size (2MB).

- **Search, Sort & Filtering**
  - Posts on the index can be sorted by title, date, or comment count.
  - Keyword search functionality included.

- **Role-Based Access**
  - `User` and `Admin` roles with different access levels.
  - Guests (non-logged-in users) can only view the index page.

- **Database Seeding**
  - Uses [Bogus](https://github.com/bchavez/Bogus) to seed fake users, posts, comments, and an admin.
  - Seeds only on first run with users having their emails as usernames.
  - Roles (`User`, `Admin`) are also seeded.

- **Account Deletion Logic**
  - When users delete their accounts, personal data is removed.
  - Their posts and comments remain but are marked as authored by "Unknown".

- **Responsive UI**
  - Built with Bootstrap and custom CSS.
  - Edit/save buttons dynamically enable based on content changes.

---

## 🛠 Tech Stack

- ASP.NET MVC (.NET Framework)
- Entity Framework (Code First)
- SQL Server
- ASP.NET Identity
- jQuery + AJAX
- XPagedList for pagination
- Bogus (seeding)
- Bootstrap 5 & CSS

---


### Prerequisites

- Visual Studio 2022+
- .NET 9 Framework 
- SQL Server 

