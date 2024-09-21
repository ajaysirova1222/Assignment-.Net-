# Recruitment Management System - Backend API

## Overview

This project is a backend API built using .NET Core 6 for a **Recruitment Management System**. The API allows users (Applicants and Admins) to create profiles, upload resumes, apply for job openings, and manage job postings. Resumes are processed via a third-party API to extract relevant information for storage and retrieval by admins.

### Key Features:
User authentication with **JWT**.
**Role-based access control** for Admins and Applicants.
**Resume upload** (PDF/DOCX only) for Applicants.
Job creation and listing for Admins.
Applicant application to job openings.
Viewing applicant data and resumes for Admins.

## Tech Stack
**.NET Core 6**: Web API Framework.
**JWT Authentication**: Token-based authentication.
**Entity Framework Core**: ORM for database operations.
** My SQL **: Database.
**POSTMAN**: API Documentation.
**Third-party API**: For resume processing (integrated to extract relevant applicant information).

## Installation

### Prerequisites
.NET Core 6 SDK
SQL Server
Postman or any API testing tool

### Steps to run the project
1. Clone the repository:
   bash
    git clone https://github.com/ajaysirova1222/Assignment-.Net-
   
2. Navigate to the project directory:
   bash
    cd recruitment-management-system
   
3. Restore the dependencies:
   bash
    dotnet restore
   
4. Set up the database:
    - Update the connection string in appsettings.json to point to your SQL Server.
    - Run the migrations to set up the database schema:
   bash
    dotnet ef database update
   
5. Run the project:
   bash
    dotnet run
   
6. Access the POSTMAN documentation at:
       
   
## API Endpoints

### Public Endpoints
**POST** /signup: Create a profile (Admin/Applicant).
**POST** /login: Authenticate users and return a JWT token.

### Applicant Endpoints
**POST** /uploadResume: Upload a resume file (PDF/DOCX only).
**GET** /jobs: Fetch a list of all job openings.
**GET** /jobs/apply?job_id={job_id}: Apply to a specific job.

### Admin Endpoints
**POST** /admin/job: Create a job opening.
**GET** /admin/job/{job_id}: Get details of a job opening, including the list of applicants.
**GET** /admin/applicants: Get a list of all applicants.
**GET** /admin/applicant/{applicant_id}: Get detailed applicant information, including extracted resume data.

## Models

### User
Name: string
Email: string
PasswordHash: string
UserType: string (Admin/Applicant)
ProfileHeadline: string
Address: string

### Profile
Applicant: User
ResumeFileAddress: string
Skills: string
Education: string
Experience: string
Phone: string

### Job
Title: string
Description: string
PostedOn: datetime
TotalApplications: int
CompanyName: string
PostedBy: User

## Authentication
The API uses **JWT (JSON Web Tokens)** for authentication. To access protected routes, include the JWT token in the Authorization header as: Bearer {token}
