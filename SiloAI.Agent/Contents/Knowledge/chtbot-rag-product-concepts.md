# Silo – Product and Product Management Business Concepts

## 1. What is a Product?

A **Product** in Silo represents a defined type of item that can be registered, identified, tracked, and managed within the system.

Before a serial or tag can be registered, the corresponding Product must already exist in Silo.

A Product contains the information needed to identify and classify the item and may also contain information about how that item is measured, packaged, or technically described.

In simple terms:

**Product = The definition and characteristics of an item managed in Silo**

---

## 2. Product Code

**Product Code** is the primary identifier of a Product.

It is used to identify the Product when registering or processing serialized items.

For example, when a new serial is registered, the Product Code determines which Product definition and related information are associated with that serial.

Product-related information may include:

- Product Type
- Brand
- Size
- Group
- Class
- Other product characteristics

The Product Code therefore provides the main connection between a Product and the serialized items associated with it.

---

## 3. Product Type

**Product Type** is used to classify Products into business categories.

For example:

- مواد اولیه
- کالای نیمه‌ساخته
- محصول نهایی

Product Type helps distinguish different categories of products and can also be used by Silo when determining how a product should be handled in different processes.

The exact Product Types available depend on the configuration of the project.

---

## 4. Product Base Information

A Product can have several basic classification attributes.

Common examples include:

- Product Type
- Brand
- Group
- Class
- Size
- Subgroup
- Status

These attributes help users classify, identify, search, and manage Products.

The exact fields and terminology may vary depending on the project.

---

## 5. Project-Specific Product Terminology

Silo can use different business terminology for the same underlying product concept depending on the project.

For example, a concept that is technically related to **Size** may be displayed as:

- سایز
- عرض کالا
- گراماژ

depending on the project's terminology.

Therefore, users may encounter different names for similar Product concepts in different Silo projects.

The terminology shown to users does not necessarily change the underlying Product concept.

---

## 6. Product Search

Silo provides Product search capabilities so users can find the required Product without manually browsing through all Products.

Depending on the available configuration, Products may be searched using information such as:

- Product Code
- Product Title
- Technical Code
- Product Type
- Brand
- Other Product attributes

This is particularly useful when an operator needs to select the correct Product during an operational process.

---

## 7. Technical Code کدفنی

A **Technical Code** is an additional identifier or code used to identify a Product.

It may be a shorter code than the official Product Code, or it may be a code that is more familiar and commonly used by the employees and operators of a specific organization.

The Technical Code provides an additional and practical way to identify or search for a Product.

For example:

A Product may have a long official **Product Code**, while employees may commonly recognize and refer to the same Product using a shorter or more familiar Technical Code such as `2142`.

Therefore:

**Product Code** → The primary identifier of the Product  
**Technical Code** → An additional identifier that may be shorter, more familiar, or more commonly used by the organization's employees for identifying and searching for the Product

A Technical Code may be used for:

- Identifying a Product
- Searching for a Product
- Finding the required Product among similar Products
- Providing a shorter identifier when applicable
- Providing an identifier that is familiar to the organization's employees
- Using a commonly recognized code when referring to a Product

The Technical Code may be used in different Silo forms or processes for Product identification or search, depending on the project configuration.

### Important Distinction: Technical Code vs. Technical Information

The assistant must not treat **Technical Code** and **Technical Information** as the same concept.

**Technical Code** is an identifier or code used to identify or search for a Product.

**Technical Information** consists of technical characteristics and specifications of a Product, such as:

- Voltage
- Power
- Body color
- Length
- Width
- Other technical specifications

For example:

If a Product has Technical Code `2142` and its technical information includes Length `100` and Width `50`:

- `2142` → Technical Code
- Length `100` and Width `50` → Technical Information

The Technical Code identifies or helps users find the Product, while Technical Information describes the Product's technical characteristics.

### Project-Specific Usage

The meaning, value, format, and usage of the Technical Code may depend on the Silo project and the terminology commonly used by the organization.

The assistant must not assume that:

- A Technical Code is always shorter than the Product Code.
- A Technical Code is always more familiar than the Product Code.
- A Technical Code always represents a manufacturer part number.
- A Technical Code always represents a model number.
- A Technical Code has the same format in every project.
- A Technical Code is used in the same way in every project.

If the required information about the Technical Code is not available in the knowledge base, the assistant must clearly state that there is not enough documented information to provide an accurate answer.

### Related Product Features

Features such as **"همانند"** or **"بازیابی اطلاعات فنی"** may be available in specific Silo forms or projects for working with Product information.

The assistant must not describe these features as part of the definition of Technical Code itself.

If the user asks specifically about **"بازیابی اطلاعات فنی"**, answer based on the documented functionality of that feature.

If the user asks specifically about **"همانند"**, answer based on the documented functionality of that option.

### Project-Specific Behavior

The value, format, required status, and usage of the Technical Code may depend on the Silo project configuration.

The assistant must not assume that:

- A Technical Code exists for every Product.
- The Technical Code always follows a specific format.
- The Technical Code is always mandatory.
- The Technical Code always represents a manufacturer part number or model number.
- The Technical Code has the same usage in every Silo project.

If the required information about Technical Code behavior is not available in the knowledge base, the assistant must clearly state that there is not enough documented information to provide an accurate answer.

---

## 8. Product Title

A Product may have a title or descriptive name that helps users identify it.

The Product Title can be based on the Product's characteristics and, depending on the project configuration, may be generated automatically.

This allows Products to have a consistent and understandable naming structure.

---

## 9. Automatic Product Code or Title Generation

Silo can support automatic generation of Product Codes or Product Titles based on predefined rules.

Instead of manually entering the complete Product Code or Title, the user may select the required Product characteristics and the system can generate the corresponding value.

For example, the generation process may use attributes such as:

- Size
- Brand
- Class
- Group
- Other Product attributes

This can help:

- Reduce data-entry errors
- Standardize Product naming
- Standardize Product identification
- Improve consistency between Product records

The exact generation rules depend on the project configuration.

---

## 10. Product Quantity

**Quantity** represents the amount associated with a registered Product unit.

Depending on the Product and project, Quantity may represent:

- A number of items
- A measurable amount
- A quantity based on a specific unit

For example:

- A shipment may contain 100 pieces.
- A pallet may contain a specific number of cartons.
- A Product may be measured in square meters.

The meaning of Quantity depends on how the Product is defined and measured in the project.

---

## 11. Default Quantity

A Product can have a configured **Default Quantity**.

When a Product is selected during registration, the system may automatically provide the configured default quantity.

The operator can then adjust the quantity when the actual amount differs from the standard value.

For example, a standard pallet may normally contain a specific quantity, while a partially filled pallet may contain less.

Therefore:

**Default Quantity = The standard quantity normally associated with a Product unit**

It does not necessarily mean that every registered unit must have exactly that quantity.

---

## 12. Primary Unit and Second Unit

Some Products may need to be represented using more than one unit of measurement.

For example:

### Tile

- Primary Unit → cartons
- Second Unit → square meters

### Paper

- Primary Unit → rolls
- Second Unit → weight or length

This allows the same Product unit to be represented using two related measurements when required.

The availability and meaning of the Second Unit depend on the Product and project configuration.

---

## 13. Shipment / Load

A **Shipment / Load** represents the traceable unit that is registered as a serial and receives a tag.

The physical meaning of this unit may differ between projects.

For example, the registered unit may be:

- A pallet
- A box
- A roll
- Another packaging or handling unit

Therefore, Shipment / Load should be understood as the **traceable registered unit**, rather than assuming that it always represents a specific type of physical package.

---

## 14. Dynamic Technical Information

In addition to standard Product attributes, Silo can support **Dynamic Technical Information**.

This information is used for technical characteristics that may differ between Products or projects.

Examples include:

- Voltage
- Power
- Body color
- Technical specifications
- Other Product-specific properties

This allows Products to have technical characteristics without requiring every possible characteristic to be part of the standard Product information.

---

## 15. Product Technical Information in Operations and Reports

Product technical information may be used in different parts of Silo.

For example, technical information may be used for:

- Displaying information on labels
- Searching for Products
- Filtering Products
- Generating reports
- Identifying Products based on technical characteristics

The exact usage depends on the configuration and capabilities available in the project.

---
## 16. Active and Inactive Products

A Product can be active or inactive.

An **Active Product** is available for operational use and selection where applicable.

An **Inactive Product** is no longer intended for new operational use.

Deactivating a Product is different from deleting it.

When a Product becomes inactive:

- Deactivation does not delete the Product or its previous information.
- Existing Product records and historical records are preserved.

This allows obsolete Products to be prevented from further operational use without losing their history.

### Deactivation Rule

If the user asks whether deactivating a Product deletes its previous information, the answer is **No**.

Deactivation is different from deletion. Previous Product information and historical records are preserved.

---

## 17. Products Requiring Double Tags

Some Products may require two tags for each registered serial.

Silo supports this requirement through the Product configuration.

When the double-tag requirement is enabled for a Product, the registration process may require two tags for the corresponding serial.

This capability is intended for Products or processes where a single physical serial requires more than one identification tag.

The exact behavior depends on the project's configuration.

---

## 18. Product and Serial/Tag Registration

A Product must exist before a serial or tag can be registered for that Product.

The general relationship is:

**Product Definition**
↓
**Product Code**
↓
**Serial Registration**
↓
**Tag Registration**

The Product provides the basic identity and characteristics associated with the registered serial or tag.

Therefore, Product management is one of the foundations of serial and tag management in Silo.

---

## 19. Product Management in Different Projects

The Product concept is consistent across Silo, but the way Products are represented can vary between projects.

Different projects may use different:

- Product Types
- Brands
- Groups
- Sizes
- Units
- Technical characteristics
- Product terminology
- Product-code generation rules

Therefore, the assistant must not assume that a specific Product attribute, value, or terminology exists in every Silo project.

---

## 20. Product Concepts at a Glance

| Concept | Simple Meaning |
|---|---|
| محصول/کالا | The definition of an item managed in Silo |
| کدکالا | The primary identifier of a Product |
| عنوان کالا | The descriptive name of a Product |
| کد فنی | An additional or short identifier for a Product |
| نوع کالا | The business category of a Product |
| برند | The Product's brand |
| گروه کالا | A broader classification of Products |
| طبقه کالا | A classification attribute of a Product |
| سایز کالا | A Product size or size-related characteristic |
| زیرگروه کالا | A more specific Product classification |
| درجه کیفیت | The amount associated with a registered Product unit |
| مقدار پیش‌فرض | The standard quantity associated with a Product |
| واحد اصلی | The main unit used to represent a Product |
| واحد دوم | An additional unit used when two measurements are required |
| محموله / بار | The traceable unit registered as a serial/tag |
| اطلاعات فنی | Product-specific technical characteristics |
| کالای فعال | A Product available for operational use |
|کالای غیرفعال | A Product no longer intended for new operational use |
| Double Tag | A requirement for two tags for a registered serial |

---

## 21. Important Accuracy Rules

The exact Product structure and behavior depend on the configuration of the Silo project.

The assistant must not assume that:

- A specific Product exists.
- A specific Product Type exists.
- A specific Brand, Group, Class, or Size exists.
- A Product supports a specific unit.
- A Product supports a Second Unit.
- A Product has a Default Quantity.
- A Product requires two tags.
- A Product Code is generated automatically.
- A Product Title is generated automatically.
- A specific Technical Code exists.
- A specific technical property is available.
- An inactive Product can be used in a specific operation.

If the required Product information is not available in the knowledge base, the assistant must clearly state that there is not enough documented information to provide an accurate answer.

The assistant must not provide speculative answers.