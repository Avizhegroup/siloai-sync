# Comprehensive and Operational Silo System Knowledge Base: Master Data and Factory Warehouse Flow

## 1. Introduction

The Silo system is used to manage product master information, product classification, product characteristics, quality, brands, sizes, and the definition of operations related to product movement within warehouses and factory environments.

Master information provides the basis for defining and managing products and helps users register and categorize products in an organized and manageable way.

Using **نوع عملیات**, users can define the conditions under which a product is moved, the source and destination of the movement, and the controls that should be applied during the operation.

The purpose of this part of the system is to provide an organized way to manage products and their operational processes.

---

# 2. Product Master Information

Master information consists of concepts used to identify, classify, and manage products in the system.

The main master information concepts are:

* نوع کالا
* طبقه کالا
* گروه کالا
* زیرگروه کالا
* برند
* سایز
* درجه کیفیت

Each of these concepts has a different purpose and should not be confused with the others.

---

# 3. نوع کالا

**نوع کالا** is used to identify the general nature of a product.

It specifies what type of product it is within the system.

Examples of نوع کالا include:

* مواد اولیه
* محصول نهایی
* قطعات یدکی
* اقلام مصرفی

In the نوع کالا section, the user can view and manage the defined product types.

Available operations include:

* Viewing the list of product types
* Creating a new نوع کالا
* Editing نوع کالا
* Deleting نوع کالا

نوع کالا is a general classification used to identify the primary nature of a product.

---

# 4. طبقه کالا

**طبقه کالا** is used for structural and specialized classification of products.

Users can define and manage different product classifications.

Information related to طبقه کالا includes items such as:

* Code
* Title
* Subtitle
* Description

In this section, the user can:

* View طبقه کالا records.
* Create a new طبقه کالا.
* Edit طبقه کالا information.
* Delete طبقه کالا.

طبقه کالا is not the same as نوع کالا.

For example, «مواد اولیه» may be a **نوع کالا**, while the specialized classification of different products within an organization may be managed through **طبقه کالا**.

---

# 5. گروه کالا

**گروه کالا** is used to organize products into main product groups.

Each group can represent a specific category of products.

Examples include:

* مواد شیمیایی
* مواد بسته‌بندی
* محصولات تولیدی
* قطعات صنعتی

The user can:

* View groups.
* Create a new گروه کالا.
* Edit group information.
* Delete a group.

گروه کالا is a classification concept and is different from نوع کالا.

---

# 6. زیرگروه کالا

**زیرگروه کالا** is used to create a more detailed classification within product groups.

گروه کالا defines the main category, while زیرگروه کالا provides a more detailed classification within that group.

For example:

**گروه کالا:** مواد شیمیایی

**زیرگروه‌ها:**

* مواد شوینده
* مواد تصفیه
* مواد افزودنی

In the زیرگروه کالا section, the user can:

* View existing subgroups.
* Create a new زیرگروه کالا.
* Edit subgroup information.
* Delete a subgroup.
* Specify the گروه کالا associated with the subgroup.

The conceptual relationship is:

**گروه کالا → زیرگروه کالا**

---

# 7. Difference Between گروه کالا and زیرگروه کالا

گروه کالا is used for the main classification, while زیرگروه کالا is used to further divide that classification.

Example:

**گروه کالا:** قطعات یدکی

**زیرگروه کالا:**

* قطعات مکانیکی
* قطعات الکتریکی
* قطعات هیدرولیکی

Therefore, a subgroup represents a more detailed category within its corresponding group.

---

# 8. برند

**برند** is used to identify the commercial brand of a product.

Multiple different products may belong to the same برند.

In the برند section, the user can:

* View the list of brands.
* Create a new برند.
* Manage brand information.
* Delete old or no-longer-used brands.

The main brand information includes:

* Brand code
* Brand title

برند is not the same as نوع کالا or گروه کالا.

For example:

**نوع کالا:** محصول نهایی
**گروه کالا:** مواد شوینده
**برند:** The relevant manufacturer brand

These concepts may all be used together to describe a product, but each represents a different aspect of the product.

## Brand Management Operations

The برند section supports full management of brand records. Users can:

* View the list of existing brands.
* Select an existing برند to view or edit its information.
* Create and add a new برند.
* Edit the information of an existing برند.
* Delete an existing برند when it is no longer needed.

These operations are part of the standard management capabilities available for master-data records.


---

# 9. سایز

**سایز** is used to manage information related to product dimensions, measurements, and physical characteristics.

This section can include information such as:

* Code
* Title
* Description
* واحد کالا
* واحد دوم
* مقدار واحد دوم در محموله
* مقدار محموله
* Weight
* Volume

The user can:

* View سایز records.
* Create a new سایز.
* Edit سایز.
* Manage سایز information.

سایز is not limited to a simple numerical measurement and can also contain information related to physical characteristics and product packaging.

---

# 10. واحد کالا

**واحد کالا** specifies the primary unit in which the quantity of a product is expressed.

Examples include:

* عدد
* کیلوگرم
* متر
* لیتر
* مترمربع

واحد کالا is different from سایز and واحد دوم.

---

# 11. واحد دوم

**واحد دوم** is used when a product can be measured or represented using another unit in addition to its primary unit.

For example, a product may:

* Use «کیلوگرم» as its primary unit,
* While also being identified as «کارتن» for packaging purposes.

In this situation, the مقدار واحد دوم can be used to specify the quantity represented by the secondary unit within a shipment.

---

# 12. مقدار واحد دوم در محموله

This value specifies how much of the secondary unit exists within a shipment.

For example:

If each shipment contains 20 cartons, the مقدار واحد دوم در محموله can be **20**.

This concept is different from مقدار محموله.

---

# 13. مقدار محموله

**مقدار محموله** specifies the quantity of product contained in a shipment.

For example, a shipment may contain:

* 1,000 kilograms of product
* 20 cartons

In this example:

* Product quantity = 1,000 kilograms
* Secondary unit quantity = 20 cartons

Therefore, these two concepts should not be confused.

---

# 14. درجه کیفیت

**درجه کیفیت** is used to specify the quality status of a product.

Examples include:

* درجه ۱
* درجه ۲
* معیوب
* ضایعات

In the درجه کیفیت section, the user can:

* View quality grades.
* Create a new درجه کیفیت.
* Edit quality grade information.
* Delete a quality grade.

The main information includes:

* Code
* Title
* Description related to the quality standard or quality status

درجه کیفیت is different from نوع کالا.

For example, a product can have نوع کالا = «محصول نهایی» while its درجه کیفیت is «درجه ۱» or «معیوب».

---

# 15. Differences Between Master Information Concepts

Each master information concept has a specific purpose:

| Concept        | Purpose                                                    |
| -------------- | ---------------------------------------------------------- |
| نوع کالا       | Identifies the general nature of the product               |
| طبقه کالا      | Provides structural and specialized product classification |
| گروه کالا      | Creates the main product category                          |
| زیرگروه کالا   | Provides a more detailed category within a group           |
| برند           | Identifies the commercial brand                            |
| سایز           | Defines size and physical characteristics                  |
| واحد کالا      | Defines the primary measurement unit                       |
| واحد دوم       | Defines an additional unit for representation or packaging |
| مقدار واحد دوم | Specifies the quantity of the secondary unit in a shipment |
| مقدار محموله   | Specifies the quantity of product in a shipment            |
| درجه کیفیت     | Specifies the quality status of the product                |

These concepts must not be mixed together or used as replacements for one another.

## Common Master Data Management Operations

Master-data sections support common management operations for their records.

Depending on the specific master-data type, users can:

* View the list of existing records.
* Select an existing record to view or edit its information.
* Create a new record.
* Edit an existing record.
* Delete an existing record.

These operations describe the general management capabilities of master-data sections. The specific information and fields available for each record depend on the corresponding master-data type.

Examples of master-data types include:

* نوع کالا
* طبقه کالا
* گروه کالا
* زیرگروه کالا
* برند
* سایز
* واحد کالا
* درجه کیفیت
* نوع عملیات

---

# 16. نوع عملیات

**نوع عملیات** is used to define how products are moved and handled within warehouses and factory environments.

نوع عملیات is not simply the name of an operation. It specifies:

* From which source the movement starts.
* To which destination the movement goes.
* Under what conditions the movement takes place.
* What controls exist during the operation.
* Which conditions or statuses are allowed for the operation.

Examples of operations in a factory environment include:

* Moving raw materials from the raw material warehouse to the production line
* Moving products from the production line to the finished product warehouse
* Moving products between warehouses
* Moving products from one location to a specified destination

---

# 17. Source and Destination in نوع عملیات

Every product movement has a **source** and a **destination**.

For example:

**انبار مواد اولیه → خط تولید**

In this example:

* Source: انبار مواد اولیه
* Destination: خط تولید

Another example:

**خط تولید → انبار محصول نهایی**

In this example:

* Source: خط تولید
* Destination: انبار محصول نهایی

The source and destination specify where the product is moved from and where it is moved to.

---

# 18. Source and Destination Warehouse Type

When defining نوع عملیات, the user can specify the type of location or warehouse from which the operation starts and the type of location or warehouse where it ends.

For example, one operation may be intended for:

**انبار مواد اولیه → خط تولید**

while another operation may be intended for:

**خط تولید → انبار محصول نهایی**

Therefore, نوع عملیات should be defined according to the actual product flow within the working environment.

---

# 19. قدرت RFID

Some operations may also include settings related to **قدرت RFID**.

This value relates to how the RFID operation is performed and should not be considered a general product characteristic.

Therefore:

**قدرت RFID ≠ Product Characteristic**

قدرت RFID is related to the conditions of the operation.

---

# 20. Document Status and Document Changes

When defining نوع عملیات, conditions related to the relevant document may also be specified.

These conditions determine which document statuses allow the operation to be performed and, where applicable, which document status changes are allowed.

The purpose is to coordinate the execution of the operation with the status of the relevant documents.

---

# 21. Operational Controls

Each **نوع عملیات** can have a set of **کنترل‌های عملیاتی** that determine the conditions under which the operation can be performed.

These controls are used to check the status of the identified products or tags, their presence at the source, registration status, quality status, document requirements, and other conditions related to the operation.

The following **کنترل‌های عملیاتی** are available:

### 1. نادیده گرفتن تگ خارج از منبع

**Ignore Tag Not in Source**

This control determines whether a tag that is not currently present at the expected source location or warehouse should be ignored.

When enabled, a tag that is outside the expected source does not cause the operation to stop or generate an error.

---

### 2. خطا در صورت نبودن تگ در منبع

**Error if Tag Is Not in Source**

This control determines whether the operation should generate an error when a tag is not present at the source location or warehouse.

When enabled, a tag that is not available at the source prevents the operation from continuing.

---

### 3. کنترل کالای فریز

**Frozen Product Control**

This control determines whether the frozen or blocked status of a product should be checked before performing the operation.

When this control is enabled, products that are in a frozen or blocked state are subject to this check during the operation.

---

### 4. کنترل کالای مردود در کنترل کیفیت

**Failed Quality Control Check**

This control determines whether products that have failed quality control are subject to a restriction during the operation.

When enabled, the quality control status of the product is checked before the operation is performed.

---

### 5. کنترل کالای بدون کنترل کیفیت

**Product Without Quality Control Check**

This control determines whether products that do not yet have a quality control result are subject to a restriction during the operation.

It is used to control whether a product without a completed quality control result can participate in the operation.

---

### 6. نادیده گرفتن تگ ثبت‌نشده

**Ignore Unregistered Tag**

This control determines whether a tag or serial that has not yet been registered in the system should be ignored.

When enabled, the unregistered status of the tag does not prevent the operation from continuing.

---

### 7. خطا در صورت ثبت‌نشده بودن تگ

**Error if Tag Is Not Registered**

This control determines whether the operation should generate an error when a tag or serial has not yet been registered.

When enabled, an unregistered tag cannot continue through the operation.

---

### 8. نادیده گرفتن عمر کمتر از حد مجاز تگ

**Ignore Tag Below Minimum Age**

This control determines whether a tag whose age is below the required minimum should be ignored.

It is used when the operation has a minimum required tag age and the system needs to determine whether a tag below that threshold should be disregarded.

---

### 9. اجباری بودن شماره سند

**Require Document Number**

This control determines whether entering the required document information is mandatory for performing the operation.

When enabled, the operation cannot be performed without the required document information.

---

### 10. تطابق دقیق اقلام با سند

**Exact Document Items Match**

This control determines whether the identified items must exactly match the items specified in the document.

When enabled:

* The identified items are compared with the items in the document.
* The identified items must exactly correspond to the document items.
* If the identified items do not exactly match the document items, the operation cannot be performed.

This control is appropriate when the operation must be performed exactly according to the items specified in a particular document.

---

### 11. کنترل کد کالا و مقدار کل سند

**Document Product and Cumulative Quantity Check**

This control is used when there is a **general document** and the related product operations are performed gradually in multiple steps.

When enabled:

* The product code of the identified product must exist among the items of the document.
* A product that is not included in the document is not allowed.
* The quantity processed through multiple operations is accumulated and compared with the total quantity allowed by the document.
* The cumulative processed quantity must not exceed the total quantity specified in the document.

For example, if a document allows **1,000 kilograms** of a specific product, the operation can be performed in multiple steps:

* First operation: 300 kilograms
* Second operation: 400 kilograms
* Third operation: 300 kilograms

The total processed quantity is 1,000 kilograms, so the operation remains within the allowed document quantity.

If the cumulative quantity exceeds **1,000 kilograms**, the operation is not allowed to continue.

Therefore, this control checks both:

* Whether the product belongs to the document.
* Whether the cumulative processed quantity exceeds the total quantity allowed by the document.

---

# 22. Example Factory Flow

A typical factory flow may be:

**مواد اولیه → انبار مواد اولیه → خط تولید → انبار محصول نهایی**

Each stage may have its own defined operation.

For example:

### Stage One

Raw materials are moved from the raw material warehouse to the production line.

### Stage Two

The production process takes place.

### Stage Three

The produced product is moved from the production line to the finished product warehouse.

The نوع عملیات for each movement specifies how and under what conditions that movement is performed.

---

# 23. Conceptual Relationship Between Product Information and Operations

For effective product management, master information and نوع عملیات have different roles.

Master information answers:

**What is this product?**

For example:

* What is the نوع کالا?
* Which گروه کالا does it belong to?
* What is its برند?
* What is its سایز?
* What is its measurement unit?
* What is its درجه کیفیت?

نوع عملیات answers:

**How and through which path is this product moved?**

For example:

* Where is it moved from?
* Where is it moved to?
* What conditions apply to the movement?
* What controls are applied during the movement?

These concepts complement each other but are not the same.

---

# 24. Complete Example

Suppose a finished product is defined in the system.

Its information may include:

* نوع کالا: محصول نهایی
* گروه کالا: محصولات تولیدی
* زیرگروه کالا: محصول نهایی کارخانه
* برند: The relevant brand
* سایز: The specified size
* واحد کالا: کیلوگرم
* واحد دوم: کارتن
* مقدار واحد دوم در محموله: 20
* درجه کیفیت: درجه ۱

After the product information is defined, an appropriate نوع عملیات can be defined for moving the product.

For example:

**خط تولید → انبار محصول نهایی**

In this case, the product information specifies **what the product is**, while نوع عملیات specifies **how the product enters the warehouse flow**.

---

# 25. Relationship Between گروه کالا and زیرگروه کالا

گروه کالا and زیرگروه کالا are used to classify products.

Their conceptual structure is:

**گروه کالا**
→ Main category

**زیرگروه کالا**
→ More detailed category within the group

Example:

**گروه کالا:**

قطعات یدکی

**زیرگروه کالا:**

* قطعات مکانیکی
* قطعات الکتریکی
* قطعات هیدرولیکی

This structure makes finding and managing products more organized and easier.

---

# 26. Relationship Between نوع کالا and Other Information

نوع کالا is a general classification and can be used together with other information to provide a more complete description of a product.

For example:

**نوع کالا:** مواد اولیه
**گروه کالا:** مواد شیمیایی
**برند:** The relevant brand
**سایز:** The specified size
**درجه کیفیت:** درجه ۱

Each of these pieces of information describes a different aspect of the product.

---

# 27. Important Conceptual Differences

To avoid confusion, the following concepts must be treated separately:

* **نوع کالا is not the same as طبقه کالا.**
* **نوع کالا is not the same as گروه کالا.**
* **گروه کالا is not the same as زیرگروه کالا.**
* **برند is not the same as نوع کالا.**
* **سایز is not the same as واحد کالا.**
* **واحد کالا is not the same as واحد دوم.**
* **مقدار واحد دوم is not the same as مقدار محموله.**
* **درجه کیفیت is not the same as نوع کالا.**
* **نوع عملیات is not the same as product classification.**
* **قدرت RFID is not a general product characteristic; it is related to the operation.**

---

# 28. Overall Functionality of the Master Information Section

The user can create and manage the information required for product management and factory operations.

The overall process may include:

1. Defining نوع کالا
2. Defining طبقه کالا
3. Defining گروه کالا
4. Defining زیرگروه کالا
5. Defining برند
6. Defining سایز and physical characteristics
7. Defining درجه کیفیت
8. Defining نوع عملیات
9. Using this information in business processes and product movements

The purpose of this structure is to provide organized and usable information for managing products and their movement within the factory environment.

---

# 29. Response Rules for This Knowledge Area

When answering questions related to master information and warehouse flows:

* Use the exact Persian names of the system concepts.
* Treat «نوع کالا», «طبقه کالا», «گروه کالا», «زیرگروه کالا», «برند», «سایز», «درجه کیفیت», and «نوع عملیات» as separate concepts.
* When the user asks about the functionality of a section, explain the documented capabilities of that section.
* If creating, editing, viewing, or deleting is documented as available for a section, explain those capabilities accordingly.
* Do not guess or introduce capabilities that are not documented in the system knowledge.
* If there is not enough information about a capability, do not provide a definitive answer about it.
* Answers must be based on the documented and actual functionality of the system.
* Explanations should be understandable and practical for the user.
* Do not explain technical implementation details, internal system structure, or implementation-related information unless the user explicitly asks about them.

---

# 30. Core Principle of This Knowledge Base

This knowledge base is designed to understand the **user-facing functionality of the Silo system**.

Its primary focus is:

* What information the user manages.
* What each concept means.
* What operations can be performed in each section.
* How different concepts differ from one another.
* How product information is used in business processes.
* How product movement operations are defined and managed.
* How the overall product flow works within warehouses and the factory environment.

Therefore, this knowledge base must be interpreted from a **user-facing and operational perspective**, not from the perspective of technical or internal system structure.
