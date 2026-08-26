# Silo Database Knowledge Base – Product, Destination, Action and Product Structure

## 1. Destination

`Destination` is one of the core concepts in the Silo system.

A Destination is not necessarily a physical location. It can represent either:

* A physical location, such as a warehouse or production line
* A process-based or abstract location, such as a process stage or a responsible organizational unit

For example, in a project such as Sinakashi, one Destination may be a production line and another Destination may be the finished-goods warehouse. Moving goods between these Destinations is considered an `Action`.

In projects where no physical movement occurs, Destination can still be used. For example, a box may physically remain in the same location while its responsibility is transferred from the warehouse to another department. In this case, the Destination can represent a new process position or responsibility.

Therefore:

> `Destination` can represent either a physical location or a process-based/abstract position.

---

## 2. Action

`Action` represents an operation or movement related to a Product or goods.

Actions have different types, referred to as `Action Type`.

`Action Type` is used to define the behavior and type of an operation associated with goods.

Each Action Type can have a specific set of controls and behaviors, such as:

* How the goods should be handled
* Whether specific controls are enabled or disabled
* Quality Control (`QC`)
* Whether creating a QC issue is allowed
* Freeze status checking (`Freeze`)
* GPS receiving or checking
* Photo receiving
* Other process-specific controls

An `Action Type` can also be associated with a specific `Product Type`.

This allows different operations to be defined for different categories of products.

For example, operations for:

* Raw materials
* Semi-finished goods
* Finished products

can be different.

---

# 3. Product

`Product` is one of the core concepts in the Silo system.

Before a `Serial` and ultimately a `Tag` can be registered, the corresponding Product and its required base information must already exist in the system.

The main Product table is:

`tbl_Product`

This table stores the list and main information of Products in the system.

A Product consists of several categories of information:

* Base Information
* Shipment / Load Unit Information
* Dynamic Technical Data

---

# 4. Product Type

`Product Type` is a key concept in the Product structure and represents the main classification of Products from a business-process perspective.

For example, in the Faradid project, Product Types can include:

* Raw Material (`Paper Base`)
* Semi-Finished Product
* Finished Product (`Cut`)

Therefore, Product Type determines which business/process category a Product belongs to.

---

# 5. Product Base Information

Before creating a Product, the required base information must be defined.

Product Base Information can include:

* Product Brand
* Product Class
* Product Group
* Product Size
* Subgroup
* Product Status
* Product Type
* Other project-specific base information

These values are managed through dedicated base-information tables.

Examples of Product base-information tables include:

* `ProductSize`
* `ProductBrand`
* `ProductGroup`
* `ProductStatus`
* Other Product base-information tables

The technical structure of these tables is fixed, but the business meaning and displayed name can vary between projects.

---

# 6. Technical Table Name vs. User-Facing Name

The technical names of database tables remain fixed, while the names displayed to users can be customized for each project.

For example:

`ProductGroup`

may be displayed to users as:

* `گروه کالا`
* `تیپ خودرو`

depending on the project.

Similarly, `ProductSize` may be displayed as:

* `سایز`
* `عرض کالا`
* `گراماژ`

depending on the project's terminology.

These terminology settings are managed through:

`Strings > Settings`

Changing a term is not necessarily limited to a single form. The configured terminology can be applied throughout the software wherever that term is used.

Therefore, when analyzing the database structure, the Agent must distinguish between:

1. The technical database/table concept
2. The user-facing business terminology

The technical table name does not change just because its business meaning or displayed name changes.

---

# 7. Product Code

`Product Code` is the primary identifier of a Product.

Product information is connected to Serial and Tag through the Product Code.

Conceptually:

```text
Product Code → Serial → Tag
```

When a Product Code is assigned to a Serial, the Product information associated with that code becomes associated with the Serial.

This can include:

* Color
* Type
* Brand
* Other Product base information

Therefore, `Product Code` is the main connection point between Product information and Serial/Tag information.

The system also provides search capabilities so that operators do not need to search through thousands of Products manually.

An operator can search using:

* Part of the Product title
* Product type
* Other Product characteristics

to find the required Product Code.

---

# 8. Technical Code / Nickname

In addition to the main `Product Code`, the system supports a shorter identifier or alias for a Product.

This is referred to as:

`Technical Code`

or

`Nickname`

The purpose is to provide a short identifier that operators can easily use in day-to-day warehouse or production operations.

For example, a Product may be commonly identified by a short code such as:

`2142`

while its main Product Code may be longer or more complex.

Therefore:

* `Product Code` is the main Product identifier.
* `Technical Code / Nickname` is a shorter operational identifier.

These two concepts should not be treated as the same field or concept.

---

# 9. Automatic Product Code and Title Generation

The system supports automatic generation of:

* `Product Code`
* Product Title

based on a predefined formula or algorithm.

In this mode, the operator does not need to manually enter the Product Code.

The operator selects the required base information, such as:

* Size
* Brand
* Class
* Other Product characteristics

and the system generates the Product Code and Product Title according to the configured algorithm.

This functionality helps to:

* Reduce human input errors
* Standardize Product Codes
* Standardize Product Titles

---

# 10. Shipment / Load Unit

The system has a concept referred to as `محموله` (Shipment / Load Unit).

In the Silo system, `محموله` is a contractual concept referring to the Serial or packaging unit for which a Tag is going to be issued.

The actual physical unit being tagged can vary between industries and projects.

For example, in the tile industry, the Tagging unit may be a `پالت` (pallet).

Therefore, in that project, a `محموله` can represent one pallet.

The concept of `محموله` is not necessarily a fixed industrial concept. It represents the unit that is registered as a Serial and tracked/tagged in the specific process.

---

# 11. Quantity

The system supports the concept of `Quantity`.

Quantity can represent either:

* A count
* A measurable amount based on a unit of measurement

For example:

* A Serial may represent a shipment containing 100 pieces.
* A shipment may be measured in square meters.

A default Quantity can be defined for a Product.

For example, in the tile industry:

* One carton may contain 1.5 square meters.
* A pallet may contain 45 cartons.
* The default Quantity of a full pallet can therefore be calculated based on the cartons contained in it.

During `Register`, the operator selects the Product Code and the system automatically displays the Product's default Quantity.

However, the operator can modify the Quantity for a specific shipment.

For example, if the last pallet is incomplete, its actual Quantity may be lower than the default Quantity of a complete pallet.

Therefore:

> Default Quantity is a predefined value, but the actual Quantity of an individual shipment can be adjusted during Register.

---

# 12. Second Unit

The system supports a second unit of measurement called `Second Unit`.

Second Unit is useful when a shipment needs to be represented using two measurement units simultaneously.

Examples:

### Tile Industry

* Number of cartons
* Square meters

### Paper Industry

* Number of rolls
* Weight or length

In projects where goods are counted individually, Second Unit values are usually considered equal to one.

In industries such as:

* Carpet (`فرش`)
* Paper (`کاغذ`)
* Tile (`کاشی`)

the Second Unit quantity can vary and should be recorded according to the actual characteristics of the shipment.

---

# 13. Accounting System Integration

The Product section of Silo can integrate with accounting systems such as:

* راهکاران
* شایگان

If a Product already exists in the accounting system, its information can be retrieved through a Web Service and automatically added to the Product table in Silo.

This integration helps to:

* Avoid defining the same Product twice
* Reduce duplicate data entry
* Keep Product information synchronized with the accounting system

---

# 14. Technical Data

In addition to Product Base Information and Shipment Information, the system supports dynamic `Technical Data`.

Technical Data contains dynamic and project-specific technical characteristics.

These characteristics can vary depending on the Product type and project requirements.

For example, a pump or device may require technical information such as:

* Input voltage
* Device power
* Body color
* Other specific technical characteristics

Technical Data is stored as `JSON` using a Key/Value structure.

Conceptually:

```text
Technical Data
    Key   → Value
    Key   → Value
    Key   → Value
```

The reason for using dynamic JSON data is that technical characteristics vary between Products and projects.

It would therefore not be practical to add fixed database columns for every possible technical characteristic.

Technical Data can be used for:

* Label / Tag printing
* Reporting
* Filtering Products
* Retrieving specific technical characteristics

For example, a report could identify how many pumps with a voltage of `24V` currently exist in inventory.

---

# 15. Product Information Categories

Product information in Silo can be divided into three main categories.

## 15.1. Base Information

Base Information contains general Product characteristics such as:

* Brand
* Group
* Class
* Size
* Subgroup
* Status
* Product Type
* Other project-specific base information

These values are generally managed through dedicated base-information tables.

---

## 15.2. Shipment Information

Shipment Information describes the unit or shipment that will be registered and tagged.

It can include:

* Weight
* Volume
* Quantity
* Default Quantity
* Main Unit
* Second Unit
* Second Unit Quantity

These values depend on how the Product is packaged and measured in the specific project.

---

## 15.3. ProductTechnicalData

Dynamic Technical Information contains project-specific and variable technical characteristics.

These values:

* Are dynamic
* Are stored as JSON
* Can vary between Products
* Can be used for Label/Tag printing
* Can be used for reporting and filtering

Therefore, Product information is not limited to a fixed set of database fields.

---

# 16. IsActive

The Product structure contains a field called:

`IsActive`

This field controls whether a Product is active or inactive.

If a Product is no longer produced or used, it can be deactivated.

When a Product is inactive:

* It is no longer shown in menus or selectable options for operators.
* Existing Product records and history are not deleted.
* Historical information remains available in records and reports.

Therefore, `IsActive` is used to deactivate old Products without deleting their historical data.

---

# 17. HasDoubleTag

The Product structure also contains a field called:

`HasDoubleTag`

This field is used for Products where each Serial must have two Tags.

If `HasDoubleTag` is enabled for a Product, the system requires two Tags to be issued when the related Serial is registered.

For example, in some projects, Products such as copper sulfate (`سولفات مس`) or zinc (`روی`) may require two Tags for each Serial.

Therefore:

```text
HasDoubleTag = true
        ↓
Serial requires two Tags
```

---

# 18. Product Database Structure Summary

Based only on the available information, the Product structure can be conceptually represented as:

```text
Product
│
├── Product Code
├── Technical Code / Nickname
├── Product Type
│
├── Base Information
│   ├── Brand
│   ├── Group
│   ├── Class
│   ├── Size
│   ├── Subgroup
│   └── Status
│
├── Shipment Information
│   ├── Quantity
│   ├── Default Quantity
│   ├── Weight
│   ├── Volume
│   ├── Main Unit
│   ├── Second Unit
│   └── Second Unit Quantity
│
├── Dynamic Technical Data
│   └── JSON Key/Value
│
├── IsActive
└── HasDoubleTag
```

This structure describes the concepts available in the provided Product information. It does not imply additional database columns or relationships that have not been explicitly described.

---

# 19. Product → Serial → Tag Relationship

Registering a Tag requires the corresponding Product to already exist.

The general conceptual flow is:

```text
Product
   ↓
Product Code
   ↓
Serial
   ↓
Tag
```

Therefore:

1. Product must exist.
2. Required Product Base Information must exist.
3. A Product Code identifies the Product.
4. The Product Code is associated with the Serial.
5. The Serial is then associated with the Tag.

The Product structure therefore provides the required foundation for the Register process.

---

# 20. Important Database Tables

The following database tables are explicitly mentioned as part of the Product structure:

| Technical Table Name                  | Purpose                                       |
| ------------------------------------- | --------------------------------------------- |
| `tbl_Product`                         | Main table containing Product information     |
| `ProductSize`                         | Stores Product Size base information          |
| `ProductBrand`                        | Stores Product Brand base information         |
| `ProductGroup`                        | Stores Product Group base information         |
| `ProductStatus`                       | Stores Product Status base information        |
| Other Product base-information tables | Store other required Product base information |

The technical names of these tables remain fixed even when their user-facing terminology changes.

For example:

```text
Technical Table:
ProductGroup

Possible User-Facing Name:
گروه کالا
or
تیپ خودرو
```

The displayed name does not change the technical table name.

---

# 21. Important Agent Rules

When answering questions about the Silo Product database structure, the Agent should follow these rules:

1. `tbl_Product` is the main table for Product information.
2. A Product must exist before its Serial or Tag can be registered.
3. Required Product Base Information must exist before the Product can be properly defined.
4. `Product Code` is the primary identifier connecting Product information to Serial and Tag.
5. `Technical Code / Nickname` is a shorter operational identifier and should not be treated as the same concept as Product Code.
6. `Product Type` is a major business/process classification of Product.
7. Base-information tables include `ProductSize`, `ProductBrand`, `ProductGroup`, `ProductStatus`, and other Product base-information tables mentioned in the system.
8. Technical database table names are fixed, while user-facing terminology can change between projects.
9. User-facing terminology is configured through `Strings > Settings`.
10. `Technical Data` is dynamic technical information stored as JSON Key/Value data.
11. Technical Data can be used for Label/Tag printing, reporting, and filtering.
12. `IsActive` is used to deactivate Products without deleting their historical records.
13. `HasDoubleTag` indicates that each Serial for that Product requires two Tags.
14. `Quantity` can have a default value, but the actual Quantity of a specific shipment can be modified during Register.
15. `Second Unit` is used when a shipment is represented using two measurement units.
16. `Destination` does not necessarily represent a physical location; it can also represent a process-based or abstract position.
17. `Action` represents an operation or movement related to goods.
18. `Action Type` defines the behavior and controls associated with an Action.
19. Action Type may include controls such as QC, Freeze, GPS, photo receiving, and other process-specific controls.
20. An `Action Type` can be associated with a `Product Type`, allowing different Product Types to have different operational behaviors.
21. The Product structure should be understood as a combination of Base Information, Shipment Information, and Dynamic Technical Data.
22. Only relationships, fields, or database structures explicitly described in this knowledge base should be assumed. Unknown database details must not be invented.
