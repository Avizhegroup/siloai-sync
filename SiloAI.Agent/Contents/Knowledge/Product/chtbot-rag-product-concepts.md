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

The Product Code connects a registered serial to the corresponding Product definition and its characteristics.

Product-related information may include:

- Product Type
- Brand
- Size
- Group
- Class
- Unit of measurement
- Other Product characteristics

Whenever possible, the Product Code should be consistent with the code used in the organization's accounting or ERP system to maintain consistency between systems.

---

## 3. Product Type

**Product Type** is used to classify Products into business categories.

For example:

- مواد اولیه
- کالای نیمه‌ساخته
- محصول نهایی

Product Type helps distinguish different categories of Products and can also be used to determine which operations are applicable to a Product.

The exact Product Types available depend on the project configuration.

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
- Quality
- Unit of measurement
- Status

These attributes help users classify, identify, search, and manage Products.

The standard Product structure contains predefined attributes. Project-specific terminology may change how these attributes are presented, but this does not necessarily mean that new standard attributes have been added.

Technical specifications that vary by Product can also be maintained separately as technical information.

---

## 5. Project-Specific Product Terminology

Silo can use different business terminology for the same underlying Product concept depending on the project.

For example, a concept related to **Size** may be displayed as:

- سایز
- عرض کالا
- گراماژ
- نوع سوخت

The appropriate meaning depends on the project. For example, in an automotive project, some standard Product attributes may be presented using vehicle-related terms.

The terminology shown to users does not necessarily change the underlying Product concept or structure.

The assistant must interpret these terms according to the documented configuration of the relevant project.

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

Product search is particularly useful when selecting the correct Product during serial registration or other operational processes.

---

## 7. Technical Code کد فنی

A **Technical Code** is an additional identifier used to identify or search for a Product.

It may be shorter or more familiar to employees than the official Product Code.

For example, a Product may have a long official Product Code while employees commonly recognize it using a shorter Technical Code such as `2142`.

- **Product Code:** The primary identifier of the Product.
- **Technical Code:** An additional identifier that may be more convenient or familiar to users.

The Technical Code's format, required status, and usage depend on the project configuration. It is not necessarily shorter than the Product Code and does not necessarily represent a manufacturer part number or model number.

### Important Distinction: Technical Code vs. Technical Information

**Technical Code** identifies or helps users find a Product.

**Technical Information** describes the Product's technical characteristics, such as voltage, power, length, width, or other specifications.

For example:

- `2142` → Technical Code
- Length `100` and Width `50` → Technical Information

Features such as «همانند» and «بازیابی اطلاعات فنی» are separate Product-management features and should not be treated as part of the definition of Technical Code.

---

## 8. Product Title

A Product may have a title or descriptive name that helps users identify it.

The Product Title can be based on the Product's characteristics and, depending on the project configuration, may be generated automatically.

A Product may also have an English title where supported.

---

## 9. Automatic Product Code or Title Generation

Silo can support automatic generation of Product Codes or Product Titles based on predefined rules.

Instead of manually entering the complete value, the user may select the required Product characteristics and the system can generate the corresponding code or title.

For example, the generation process may use attributes such as:

- Size
- Brand
- Class
- Group
- Other Product attributes

This can help reduce data-entry errors and standardize Product identification and naming.

The exact generation rules depend on the project configuration.

---

## 10. Product Quantity

**Quantity** represents the amount associated with a Product or its registered packaging unit.

Depending on the Product and project, quantities may represent:

- A number of pieces
- A number of cartons or other packages
- A measurable amount, such as square meters or weight

Some Products are managed by count only. Others require both a count and a measurable quantity.

For example, a tile pallet may be tracked by the number of cartons it contains and the total area of the tiles.

The meaning of Quantity depends on how the Product is defined and measured in the project.

---

## 11. Default Quantity

A Product can have a configured **Default Quantity**.

When a serial or packaging unit is registered, the system may use this value as the initial quantity for that unit.

The operator may adjust the quantity when the actual amount differs from the standard value, where the process permits it.

For example, a standard pallet may contain a defined quantity, while a partially filled pallet contains less.

**Default Quantity = The standard quantity initially assigned to a registered unit.**

It does not necessarily mean that every registered unit must have exactly that quantity.

---

## 12. Primary Unit, Second Unit, and Packaging Quantity

Some Products need to be represented using two related units of measurement.

For example:

- **Primary Unit:** Carton
- **Second Unit:** Square meter

A Product may also define how many second units are contained in a standard package or handling unit.

These concepts must be distinguished:

- **Value per Second Unit:** The amount represented by one second unit.
- **Count in Pack:** The number of second units in the standard package.
- **Pack Value:** The total amount represented by one standard package or registered unit.

### Example: Tiles

Suppose one carton contains `1.5 m²` of tiles and a standard pallet contains `45` cartons.

- Value per carton: `1.5 m²`
- Cartons per pallet: `45`
- Total standard pallet quantity: `67.5 m²`

Calculation:

`1.5 × 45 = 67.5 m²`

The system can use the standard Pack Value as the initial quantity for a newly registered pallet. If the last pallet contains only `20 m²`, its actual quantity may differ from the standard value where the process allows an override.

Other packaging defaults, such as weight or volume, may also be configured where supported.

The availability and meaning of these values depend on the Product and project configuration.

---

## 13. Shipment / Load

A **Shipment / Load** represents the traceable unit that is registered as a serial and receives a tag.

The physical meaning of this unit may differ between projects.

For example, the registered unit may be:

- A pallet
- A box
- A roll
- Another packaging or handling unit

A registered unit may contain multiple smaller units and may represent a measurable quantity rather than a single physical item.

Therefore, Shipment / Load should be understood as the **traceable registered unit**, rather than assuming that it always represents a specific type of package.

---

## 14. Dynamic Technical Information

In addition to standard Product attributes, Silo can support **Dynamic Technical Information** for technical characteristics that vary between Products or projects.

Examples include:

- Voltage
- Power
- Body color
- Length and width
- Other technical specifications

These characteristics describe the Product and may be used without making every possible technical characteristic part of the standard Product structure.

**Product-level technical information** describes the Product definition and generally applies to units of that Product.

This is different from information recorded for an individual serial or package. For example, a Product may have a standard manufacturing specification, while a particular serial may need to record which furnace produced it.

The assistant must not assume that every technical characteristic belongs to the Product definition; some information may be specific to an individual registered unit.

---

## 15. Product Technical Information in Operations and Reports

Product technical information may be used in different parts of Silo, depending on the project configuration.

Examples include:

- Displaying information on labels
- Searching for or filtering Products
- Generating reports and dashboards
- Identifying Products based on technical characteristics

Some technical information may also be used in operational processes where supported.

The exact usage depends on the configuration and capabilities available in the project.

---

## 16. Active and Inactive Products

A Product can be active or inactive.

An **Active Product** is available for operational use and selection where applicable.

An **Inactive Product** is no longer intended for new operational use.

Deactivating a Product is different from deleting it.

When a Product becomes inactive:

- The Product and its previous information are not deleted.
- Existing records and historical information are preserved.
- The Product should not be assumed to be available for new registration or other operations.

### Deactivation Rule

If the user asks whether deactivating a Product deletes its previous information, the answer is **No**.

Deactivation preserves existing information and history; it is not deletion.

---

## 17. Products Requiring Double Tags

Some Products may require two tags for each registered serial.

Silo supports this requirement through the Product configuration.

When the double-tag requirement is enabled for a Product, its registration process may require two tags for the corresponding serial.

This capability is intended for Products or processes where a single registered unit requires more than one identification tag.

The exact behavior depends on the project's configuration.

---

## 18. Product and Serial/Tag Registration

A Product must exist before a serial or tag can be registered for that Product.

The general relationship is:

**Product Definition → Product Code → Serial Registration → Tag Registration**

The Product provides the identity and characteristics associated with the registered serial or tag.

The Product Code connects the registered unit to the appropriate Product definition.

Therefore, Product management is one of the foundations of serial and tag management in Silo.

---

## 19. Product Management in Different Projects

The Product concept is consistent across Silo, but the way Products are represented can vary between projects.

Different projects may use different:

- Product Types
- Brands
- Groups and Subgroups
- Sizes and quality classifications
- Units of measurement
- Packaging quantities
- Technical characteristics
- Product terminology
- Product-code and title generation rules

Products may be created manually, imported from an accounting or ERP system through an available integration, or imported from Excel where that capability is enabled.

When integration is configured, Product information may be supplied by the external system rather than entered manually in Silo.

The assistant must not assume that every project supports the same integration or import method.

---

## 20. Product Concepts at a Glance

| Concept | Simple Meaning |
|---|---|
| محصول/کالا | The definition of an item managed in Silo |
| کد کالا | The primary identifier of a Product |
| عنوان کالا | The descriptive name of a Product |
| کد فنی | An additional identifier for a Product |
| نوع کالا | The business category of a Product |
| برند | The Product's brand |
| گروه کالا | A broader classification of Products |
| طبقه کالا | A Product classification attribute |
| سایز کالا | A size-related Product attribute whose displayed meaning may vary by project |
| زیرگروه کالا | A more specific Product classification |
| درجه کیفیت | A Product quality classification |
| واحد اصلی | The main unit used to represent a Product |
| واحد دوم | An additional unit used when two measurements are required |
| مقدار هر واحد دوم | The measurable amount represented by one second unit |
| تعداد در بسته | The number of second units in a standard package |
| مقدار بسته | The total standard quantity represented by a package or registered unit |
| مقدار پیش‌فرض | The standard initial quantity assigned during registration |
| محموله / بار | The traceable unit registered as a serial/tag |
| اطلاعات فنی کالا | Technical characteristics describing a Product |
| اطلاعات اختصاصی سریال | Information specific to an individual registered unit |
| کالای فعال | A Product available for operational use where applicable |
| کالای غیرفعال | A Product not intended for new operational use; its history is preserved |
| Double Tag | A requirement for two tags for a registered serial |

---

## 21. Important Accuracy Rules

The exact Product structure and behavior depend on the configuration of the Silo project.

The assistant must not assume that:

- A specific Product or Product Type exists.
- A specific Brand, Group, Class, Size, or quality value exists.
- A Product supports a second unit or packaging quantity.
- Every Product is measured by quantity rather than count only.
- Every registered package has the standard quantity.
- A Product Code or Title is generated automatically.
- A Technical Code exists for every Product.
- Every technical characteristic belongs to the Product definition rather than an individual serial.
- A Product requires two tags.
- A specific import or integration method is enabled.
- An inactive Product can be used in a specific operation.

If the required Product information is not available in the knowledge base, the assistant must clearly state that there is not enough documented information to provide an accurate answer.

The assistant must not provide speculative answers.