﻿(*
Copyright (c) 2013-2014 FSharp.ViewModule Team

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
*)

namespace FSharp.ViewModule.Tests

open NUnit.Framework
open NUnit.Framework.Constraints
open FsUnit
open FSharp.ViewModule
open System.ComponentModel

open FSharp.ViewModule.TypeProvider
open FSharp.ViewModule.Tests.Model

// Builds ViewModels based on "Model" assembly, using "MvvmCross" based classes as the base classes
type MvvmCrossViewModels = ViewModelProvider<"FSharp.ViewModule.Tests.Model", "FSharp.ViewModule.MvvmCross", "FSharp.ViewModule.MvvmCross.ViewModuleTypeSpecification">

// Builds ViewModels based on "Model" assembly, using the default base classes in FSharp.ViewModule.Core
type ViewModels = ViewModelProvider<"FSharp.ViewModule.Tests.Model">

module SpecificTests =
    [<Test>]
    let ``Can create an instance of Home ViewModule`` () =
        let home = ViewModels.Home()
        home.Fullname |> should equal " "  

    [<Test>]
    let ``Explicitly adding in existing tracking name shouldn't cause extra PropertyChanged events``() =
        let home = ViewModels.Home()
        
        // Try to force an "extra" property dependency
        let vm = home :> FSharp.ViewModule.IViewModel
        vm.DependencyTracker.AddPropertyDependency(<@@ home.Fullname @@>, <@@ home.Firstname @@>)

        let resArr = ResizeArray<string>()
        use subscription = (home :> INotifyPropertyChanged).PropertyChanged.Subscribe(fun args -> resArr.Add(args.PropertyName))

        home.Firstname <- "Foo"
        resArr.Count |> should equal 2
        resArr |> should contain "Firstname"
        resArr |> should contain "Fullname"

    [<Test>]
    let ``Explicitly adding in existing tracking name shouldn't cause extra PropertyChanged events with MvvmCross``() =
        let home = MvvmCrossViewModels.Home()
        home.ShouldAlwaysRaiseInpcOnUserInterfaceThread(false) // Required for MvvmCross to not delay the prop changed events

        // Try to force an "extra" property dependency
        let vm = home :> FSharp.ViewModule.IViewModel
        vm.DependencyTracker.AddPropertyDependency(<@@ home.Fullname @@>, <@@ home.Firstname @@>)

        let resArr = ResizeArray<string>()
        use subscription = (home :> INotifyPropertyChanged).PropertyChanged.Subscribe(fun args -> resArr.Add(args.PropertyName))

        home.Firstname <- "Foo"
        resArr.Count |> should equal 2
        resArr |> should contain "Firstname"
        resArr |> should contain "Fullname"

    [<Test>]
    let ``Setting names in Home ViewModule should raise Property Changed`` () =
        let home = ViewModels.Home()
        let resArr = ResizeArray<string>()
        use subscription = (home :> INotifyPropertyChanged).PropertyChanged.Subscribe(fun args -> resArr.Add(args.PropertyName))

        home.Firstname <- "Foo"
        home.Lastname <- "Bar"

        resArr.Count |> should be (greaterThanOrEqualTo 4)
        resArr |> should contain "Firstname"
        resArr |> should contain "Lastname"
        resArr |> should contain "Fullname"

    [<Test>]
    let ``Setting names in Home ViewModule should raise Property Changed in MvvmCross`` () =
        let home = MvvmCrossViewModels.Home()
        home.ShouldAlwaysRaiseInpcOnUserInterfaceThread(false) // Required for MvvmCross to not delay the prop changed events
        let resArr = ResizeArray<string>()
        use subscription = (home :> INotifyPropertyChanged).PropertyChanged.Subscribe(fun args -> resArr.Add(args.PropertyName))

        home.Firstname <- "Foo"
        home.Lastname <- "Bar"

        resArr.Count |> should be (greaterThanOrEqualTo 4)
        resArr |> should contain "Firstname"
        resArr |> should contain "Lastname"
        resArr |> should contain "Fullname"

    [<Test>]
    let ``Click in command should increment ClickCount`` () =
        let home = ViewModels.Home()
        home.ClickCount |> should equal 0
        home.Click.Execute(null)
        home.ClickCount |> should equal 1

    [<Test>]
    let ``ClickCount should increment when passing in state`` () =
        let state = { Firstname = "Foo"; Lastname = "Bar" ; ClickCount = 3 }
        let home = ViewModels.Home(state)
        home.ClickCount |> should equal 3
        home.Click.Execute(null)
        home.ClickCount |> should equal 4

    [<Test>]
    let ``ClickCount should increment when passing in state with MvvmCross`` () =
        let state = { Firstname = "Foo"; Lastname = "Bar" ; ClickCount = 3 }
        let home = MvvmCrossViewModels.Home(state)
        home.ClickCount |> should equal 3
        home.Click.Execute(null)
        home.ClickCount |> should equal 4
