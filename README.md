# Adventurer's Guild
A full stack task management app using C#, ASP.NET Core, EF Core, FluentAPI, Flutter, and PostgreSQL

## Idea
The motivation and origin of this app comes from me wishing I could reduce my habits for procrastination by creating a task management app. However, most task management apps feel like chores and are not fun and interactive. There are rarely any incentives because one could easily be dishonest to themselves. Wanting to combat this issue,  **Adventurer's Guild** was born.

### Feature Ideas
* Fun and interactive
* Rewards for completing tasks (currency)
* Accountability System
* Silly naming conventions to avoid negative connotation
* Make it feel like a game

## Uniqueness
Ultimately the coolest idea I had was the accountability system. The premise was to prevent users from cheating themselves and making tasks/marking tasks as complete just to get in-app currency. Thus I thought of a system where one would only receive part of the reward when completing a task, and they would have to send proof of their completion to a group of friends (called a party). Their fellow party members would then verify that the user did indeed complete the task. This not only keeps users accountable but also encourages and increases overall motivation in the group.

## Planning the Database
I knew the database design was going to be a bit complicated, and so I decided to create a design for the database even though I decided on taking the code-first approach.
![Imgur](https://i.imgur.com/kbuHwoJ.png "Database Design")

## Code-First Migrations
Having planned out the database really well, I was able to easily set up the tables for the database using Entity Framework Core.

![Imgur](https://i.imgur.com/zJWDb9J.png "Code-First Tables")
![Imgur](https://i.imgur.com/1IxROaJ.png "Example Model")

## Issues with Entity Framework
Though EF Core is a great system with a robust attribute system, one thing I found it struggled greatly with was keyless tables, unconventional primary keys, and others. Luckily, there were ways around the issues.

* FluentAPI allows you to specify in more detail what you want the tables to look like:
  ![Imgur](https://i.imgur.com/j9SrIrH.png)
* Keyless entities (basically what EF Core calls tables) can be handled with raw SQL queries:
  ![Imgur](https://i.imgur.com/VMrOANV.png)
  * As opposed to using LINQ:
  ![Imgur](https://i.imgur.com/b9yEi1m.png)
  ![Imgur](https://i.imgur.com/owbLhuY.png)
  ![Imgur](https://i.imgur.com/e2AP6QQ.png)

## Comments on writing a RESTful API backend with C#
Overall ASP.NET Core and EF Core were fairly comfortable to navigate even as a complete beginner and I was able to achieve what I wanted pretty easily :)
