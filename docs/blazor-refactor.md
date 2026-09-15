---
name: blazor-refactor
description: "Refactor Blazor components in the Silo WMS project: split @code blocks into .razor.cs code-behind files, apply [Inject] DI, enforce event naming conventions, add JsonSerializerContext, wire up RfidConnectApi, add TelerikLoaderContainer, and ensure .editorconfig compliance."
---

# /blazor-refactor

Refactor one or more Blazor components in the Silo project to meet the project's architecture standards.

## Usage

```
/blazor-refactor                    # refactor the current .razor file
/blazor-refactor <ComponentName>    # refactor a specific component by name
/blazor-refactor --analyze          # analyze only, list issues without making changes
```

---

## What You Must Do When Invoked

1. **Analyze** the component for all violations listed below.
2. **Produce** the corrected `.razor` and `.razor.cs` files, plus any needed `GlobalUsings.cs` / `_Imports.razor` / `JsonSerializerContext` additions.
3. **List** all changes made at the end.

---

## Required Standards

### 1. Code-Behind Separation (non-negotiable)

- **No `@code { }` blocks** in `.razor` files — ever.
- Move all C# logic to a `.razor.cs` partial class file.
- The `.razor` file contains only markup and `@using` / `@inject` / `@inherits` directives.

```
MyComponent.razor       ← markup only
MyComponent.razor.cs    ← partial class with all logic
```

### 2. Dependency Injection

Use `[Inject]` attribute properties — never constructor parameters in Blazor components:

```csharp
[Inject] public RfidConnectApi Api { get; set; }
[Inject] public IMapper Mapper { get; set; }
[Inject] public IExcelExport ExcelExporter { get; set; }
[Inject] public IFormalDataCache FormalCache { get; set; }
```

### 3. Base Class

All pages must inherit `SiloBasePage` (from `Silo.Identity.Client`):

```razor
@inherits SiloBasePage
```

Override `SiloInitializer()` instead of `OnInitializedAsync()` where access checks are needed:

```csharp
protected override async Task SiloInitializer()
{
    IsLoading = true;
    // load data...
    IsLoading = false;
}
```

### 4. Event Naming Convention

Format: `On[Name][EventName]`

✅ `OnClearClick`, `OnSaveSubmit`, `OnWarehouseSelect`, `OnDataChanged`  
❌ `ClearClick`, `HandleSave`, `WarehouseSelected`

### 5. API Calls via RfidConnectApi

All HTTP calls from Blazor must use `RfidConnectApi` — never raw `HttpClient`.

```csharp
// By stored procedure name + JsonSerializerContext:
var result = (await Api.PostAsyncByContext<List<GetAllWarehousesVm>>(
    "SGetAllWarehouses", new GetAllWarehousesVmContext())).Value;

// By URI + parameters + JsonSerializerContext:
var result = (await Api.PostAsyncByUriAndContext<List<GetReportFormatsByPathVm>>(
    "/api/report-formats",
    new KeyValuePair<string, object>("id", SelectedFormatId),
    new GetReportFormatsByPathVmContext())).Value;
```

Always wrap calls with `IsLoading = true` / `IsLoading = false`.

### 6. Loading State (mandatory)

Every component that makes API calls must have a `TelerikLoaderContainer`:

```razor
<TelerikLoaderContainer Visible="IsLoading"
                        LoaderPosition="@LoaderPosition.End"
                        LoaderType="LoaderType.InfiniteSpinner"
                        Text="@TextResources.APP_StringKeys_Loading">
</TelerikLoaderContainer>
```

```csharp
public bool IsLoading = false;
```

### 7. Data Grids

Use `TelerikGrid` for all data list views — never plain HTML tables for business data.

```razor
<TelerikGrid Data="Items"
             PageSize="10"
             Navigable="true"
             Pageable="true"
             Sortable="true">
    <GridColumns>
        <GridColumn Field="@nameof(MyVm.Code)" Title="@TextResources.APP_StringKeys_Code" Width="100px" />
        <GridColumn Field="@nameof(MyVm.Title)" Title="@TextResources.APP_StringKeys_Title" Width="200px" />
    </GridColumns>
</TelerikGrid>
```

### 8. JSON Serialization

Add a `JsonSerializerContext` for each response VM used via `RfidConnectApi`:

```csharp
[JsonSerializable(typeof(ApiResponse<List<MyVm>>))]
public partial class MyVmContext : JsonSerializerContext { }
```

Skip contexts for primitive types (`int`, `bool`, `string`).

### 9. Localization

Never hardcode user-visible strings. Use `TextResources`:

```razor
@TextResources.APP_StringKeys_Title
```

```csharp
Label = TextResources.APP_StringKeys_Choose
```

### 10. File Organization

- `GlobalUsings.cs` — shared C# usings for the project
- `_Imports.razor` — shared Blazor `@using` directives
- Use **file-scoped namespaces** in all `.razor.cs` files
- Implement `IDisposable` when subscribing to events or long-lived resources

### 11. Code-Behind Layout Convention

Organize `.razor.cs` members in this order with region comments:

```csharp
// variables part
public bool IsLoading = false;
public List<MyVm> Items;
// variables part

// services part
[Inject] public RfidConnectApi Api { get; set; }
// services part

// parameters part
[Parameter] public int? SelectedId { get; set; }
[Parameter] public EventCallback<int> OnItemSelected { get; set; }
// parameters part

// components references part
public Modal ModalRef { get; set; }
// components references part
```

### 12. .editorconfig Compliance

- File-scoped namespaces (enforced as error)
- No unused `using` directives (enforced as error)
- No unused local variables (enforced as error)
- UTF-8 BOM encoding on `.cs` files
- No `this.` qualifiers

---

## Output Format

Produce:
1. **`{Component}.razor`** — cleaned markup file
2. **`{Component}.razor.cs`** — code-behind partial class
3. **`GlobalUsings.cs` additions** (if any new namespaces required)
4. **`_Imports.razor` additions** (if any new Blazor usings required)
5. **`JsonSerializerContext`** classes (if API calls added/changed)
6. **Change summary** — bullet list of every issue fixed
