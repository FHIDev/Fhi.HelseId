# Fhi.AuthControllers

This package contains an account controller with endpoints for account login and logout.
It depends on the [Fhi.HelseId](https://www.nuget.org/packages/Fhi.HelseId) package.

The intention of the package is to save the developer from writing code that is always the same.
There is no need for any other initialization that adding the package to your project.

## Installation

Just add the package to your project from [Nuget](https://www.nuget.org/packages/Fhi.AuthControllers).

## Description

### AccountController

The package adds a controller named `AccountController` with the following GET endpoints:

* Login
* Logout
* Ping

The last `Ping` endpoint is used to check if the controller is installed.`

### UserController

It also includes a UserController with a method `User` that displays information about the logged in user.

### Default Html files

The package also adds four default pages, for `Error`, `Forbidden`, `LoggedOut` and `StatusCode` in the wwwroot folder.