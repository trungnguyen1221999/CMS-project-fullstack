## **Building a CMS with .NET 8 & React** 

## **1. Design Patterns & Architecture Architecture** 

- **Clean Architecture** 

**==> picture [99 x 13] intentionally omitted <==**

**----- Start of picture text -----**<br>
Design Patterns<br>**----- End of picture text -----**<br>


**==> picture [117 x 11] intentionally omitted <==**

**----- Start of picture text -----**<br>
● Repository Pattern<br>**----- End of picture text -----**<br>


- **Unit of Work** 

- **Dependency Injection** 

## **2. CMS Project Overview Admin Portal (Management)** 

- Role Management 

- User Management (Assign permissions, role mapping, change email, reset password) 

- Category Management 

- Post Management (Approve, publish posts) 

- Series Management 

- Comment Management 

## **News Portal (Public Website)** 

- Homepage Display 

- Post List & Category Views 

- Post Details 

- Authentication (Sign up, Sign in) 

- Third-Party Authentication (Google, Facebook) 

- Author Dashboard (View published posts, published series, royalty/payout info) 

- Notification System 

## **3. Business Logic & Database Design** 

- Analyze Project Business Requirements 

- Database Analysis & Design 

## **4. Backend Core Setup** 

- Create Entities for Posts 

- Integrate ASP.NET Core Identity 

- Configure Database Migrations 

- Data Seeding on Application Startup 

- Configure Repository Pattern and Unit of Work 

- Implement and Test PostRepository 

- Create Base Pagination and AutoMapper Configuration 

- Implement and Test PostController 

- Authentication & Authorization with API JWT Token (Part 1) 

## **5. Module Implementation** 

## **Permission Management** 

- Manage and enforce system permissions 

## **User Management** 

- User Listing 

- CRUD Operations for Users 

- Assign Users to Roles 

- Change Email 

- Set/Reset Password 

## **Post Category Management** 

- Create API Controllers 

- Create Frontend Components (Include in Content Module) 

- Configure Frontend Routing 

- Apply Permissions 

- Generate Client API 

- _Database Mapping:_ Table: PostCategories | Entity: PostCategory 

## **Post & Series Management with Approval Workflow** 

- Post Management (Rich Text Editor and Image Upload) 

- Track Status Change History 

- Add Posts to Series 

- Series Management (Manage Series & Post-to-Series mapping) 

- **Approval Workflow:** 

   - Separation of roles between Authors (Creators) and Reviewers (Editors). 

   - Authors can only create posts, save drafts, and submit for review. 

   - Reviewers can either **Approve** (Publish) the post or **Reject** it with a comment. 

   - Draft posts are strictly private and visible only to the creator. 

## **Royalty & Payout Management** 

- **Technology Stack:** Use Dapper for raw SQL database queries. 

- **Backend Implementation:** 

   - Add Balance field to User entity to track current balance. 

   - Add LoyaltyAmountPerPost field to User entity to define payout rate per published post. 

   - Add PaidDate field to Posts entity to log payment dates. 

   - Create Transactions table to record transaction history. 

   - Implement LoyaltyService to handle reporting and payment processing. 

## **6. Deployment & DevOps** 

- Configure Deployment Environment 

- Deploy Applications (Backend & Frontend) 

