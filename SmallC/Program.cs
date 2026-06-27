// <copyright file="Program.cs" company="Soli Deo Gloria Apps">
// Copyright (c) Soli Deo Gloria Apps. All rights reserved.
// </copyright>

using SmallC.Cc4;

Console.WriteLine("Hello, World!");

var backend = new BackEnd(Console.Out);
await backend.HeaderAsync();
