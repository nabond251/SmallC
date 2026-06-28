// <copyright file="Program.cs" company="Soli Deo Gloria Apps">
// Copyright (c) Soli Deo Gloria Apps. All rights reserved.
// </copyright>

using SmallC.Cc;
using SmallC.Cc4;

var storage = new Storage(new([], []));
var backend = new BackEnd(storage, Console.Out);
await backend.HeaderAsync().ConfigureAwait(true);
