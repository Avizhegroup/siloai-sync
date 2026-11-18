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

In نوع کالا form, the user can view and manage the defined product types.

Information related to نوع کالا form includes items such as:

* کد
* عنوان

Available operations in نوع کالا form include:

* Viewing the list of product types
* Creating a new نوع کالا
* Editing نوع کالا

نوع کالا is a general classification used to identify the primary nature of a product.

**Excel Import and Export Limitations:** Silo currently does not support importing نوع کالا records from Excel files or exporting existing نوع کالا records to Excel or other file formats.
If the user asks whether this is possible or how to perform it (e.g., "How can I export نوع کالا records to Excel?"), clearly state that this functionality is currently not supported.
Do not list sample records, provide alternative data as an export, or imply that the functionality exists.

1. View and Select Existing Records (دکمه طوسی - آیکون لیست): Click this button to display the list of registered نوع کالا records. The user can browse the list and select the desired record.
2. This form does not have a delete option.
3. Reset Form (دکمه زرد - آیکون رفرش): Click this button to clear the entered information and reset the form, allowing the user to start entering new information.
4. Save Information (دکمه سرمه ای - آیکون save): Click this button to save the entered information and register the new نوع کالا record.

---

# 4. طبقه کالا

**طبقه کالا** is used for structural and specialized classification of products.

Users can define and manage different product classifications.

Information related to طبقه کالا form includes items such as:

* کد
* عنوان
* عنوان فرعی
* توضیحات

In طبقه کالا form, the user can:

* View طبقه کالا records.
* Create a new طبقه کالا.
* Edit طبقه کالا information.
* Delete طبقه کالا.

طبقه کالا is not the same as نوع کالا.

For example, «مواد اولیه» may be a **نوع کالا**, while the specialized classification of different products within an organization may be managed through **طبقه کالا**.

**Excel Import and Export Limitations:** Silo currently does not support importing طبقه کالا records from Excel files or exporting existing طبقه کالا records to Excel or other file formats.
If the user asks whether this is possible or how to perform it (e.g., "How can I export طبقه کالا records to Excel?"), clearly state that this functionality is currently not supported.
Do not list sample records, provide alternative data as an export, or imply that the functionality exists.

1. View and Select Existing Records (دکمه طوسی - آیکون لیست): Click this button to display the list of registered طبقه کالا records. The user can browse the list and select the desired record.
2. Delete Record (دکمه قرمز - آیکون زباله): Click this button to delete the selected طبقه کالا record.
3. Reset Form (دکمه زرد - آیکون رفرش): Click this button to clear the entered information and reset the form, allowing the user to start entering new information.
4. Save Information (دکمه سرمه ای - آیکون save): Click this button to save the entered information and register the new طبقه کالا record.

---

# 5. گروه کالا

**گروه کالا** is used to organize products into main product groups.

Each group can represent a specific category of products.

Examples include:

* مواد شیمیایی
* مواد بسته‌بندی
* محصولات تولیدی
* قطعات صنعتی

Information related to گروه کالا form includes items such as:

* کد
* عنوان

In گروه کالا form, the user can:

* View groups.
* Create a new گروه کالا.
* Edit group information.
* Delete a group.

گروه کالا is a classification concept and is different from نوع کالا.

**Excel Import and Export Limitations:** Silo currently does not support importing گروه کالا records from Excel files or exporting existing گروه کالا records to Excel or other file formats.
If the user asks whether this is possible or how to perform it (e.g., "How can I export گروه کالا records to Excel?"), clearly state that this functionality is currently not supported.
Do not list sample records, provide alternative data as an export, or imply that the functionality exists.

1. View and Select Existing Records (دکمه طوسی - آیکون لیست): Click this button to display the list of registered گروه کالا records. The user can browse the list and select the desired record.
2. Delete Record (دکمه قرمز - آیکون زباله): Click this button to delete the selected گروه کالا record.
3. Reset Form (دکمه زرد - آیکون رفرش): Click this button to clear the entered information and reset the form, allowing the user to start entering new information.
4. Save Information (دکمه سرمه ای - آیکون save): Click this button to save the entered information and register the new گروه کالا record.

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

Information related to زیرگروه کالا form includes items such as:

* کد
* عنوان
* عنوان فرعی
* توضیحات
* انتخاب گروه کالا

In the زیرگروه کالا form, the user can:

* View existing subgroups.
* Create a new زیرگروه کالا.
* Edit subgroup information.
* Delete a subgroup.
* Specify the گروه کالا associated with the subgroup.

The conceptual relationship is:

**گروه کالا → زیرگروه کالا**

**Excel Import and Export Limitations:** Silo currently does not support importing زیرگروه کالا records from Excel files or exporting existing زیرگروه کالا records to Excel or other file formats.
If the user asks whether this is possible or how to perform it (e.g., "How can I export زیرگروه کالا records to Excel?"), clearly state that this functionality is currently not supported.
Do not list sample records, provide alternative data as an export, or imply that the functionality exists.

1. View and Select Existing Records (دکمه طوسی - آیکون لیست): Click this button to display the list of registered زیرگروه کالا records. The user can browse the list and select the desired record.
2. Delete Record (دکمه قرمز - آیکون زباله): Click this button to delete the selected زیرگروه کالا record.
3. Reset Form (دکمه زرد - آیکون رفرش): Click this button to clear the entered information and reset the form, allowing the user to start entering new information.
4. Save Information (دکمه سرمه ای - آیکون save): Click this button to save the entered information and register the new زیرگروه کالا record.

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

In the برند form, the user can:

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

**Excel Import and Export Limitations:** Silo currently does not support importing برند کالا records from Excel files or exporting existing برند کالا records to Excel or other file formats.
If the user asks whether this is possible or how to perform it (e.g., "How can I export برند کالا records to Excel?"), clearly state that this functionality is currently not supported.
Do not list sample records, provide alternative data as an export, or imply that the functionality exists.

1. View and Select Existing Records (دکمه طوسی - آیکون لیست): Click this button to display the list of registered برند کالا records. The user can browse the list and select the desired record.
2. Delete Record (دکمه قرمز - آیکون زباله): Click this button to delete the selected برند کالا record.
3. Reset Form (دکمه زرد - آیکون رفرش): Click this button to clear the entered information and reset the form, allowing the user to start entering new information.
4. Save Information (دکمه سرمه ای - آیکون save): Click this button to save the entered information and register the new برند کالا record.

## Brand Management Operations

The برند form supports full management of brand records. Users can:

* View the list of existing brands.
* Select an existing برند to view or edit its information.
* Create and add a new برند.
* Edit the information of an existing برند.
* Delete an existing برند when it is no longer needed.

These operations are part of the standard management capabilities available for master-data records.


---

# 9. سایز

**سایز** is used to manage information related to product dimensions, measurements, and physical characteristics.

In سایز form can include information such as:

* کد
* عنوان
* توضیحات
* واحد کالا
* مقدار واحد دوم
* مقدار واحد دوم در محموله
* مقدار محموله
* وزن محموله
* حجم محموله

In the سایز کالا form, the user can:

* View سایز records.
* Create a new سایز.
* Edit سایز.
* Manage سایز information.

سایز is not limited to a simple numerical measurement and can also contain information related to physical characteristics and product packaging.

**Excel Import and Export Limitations:** Silo currently does not support importing سایز کالا records from Excel files or exporting existing سایز کالا records to Excel or other file formats.
If the user asks whether this is possible or how to perform it (e.g., "How can I export سایز کالا records to Excel?"), clearly state that this functionality is currently not supported.
Do not list sample records, provide alternative data as an export, or imply that the functionality exists.

1. View and Select Existing Records (دکمه طوسی - آیکون لیست): Click this button to display the list of registered سایز کالا records. The user can browse the list and select the desired record.
2. Delete Record (دکمه قرمز - آیکون زباله): Click this button to delete the selected سایز کالا record.
3. Reset Form (دکمه زرد - آیکون رفرش): Click this button to clear the entered information and reset the form, allowing the user to start entering new information.
4. Save Information (دکمه سرمه ای - آیکون save): Click this button to save the entered information and register the new سایز کالا record.

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

In the درجه کیفیت form, the user can:

* View quality grades.
* Create a new درجه کیفیت.
* Edit quality grade information.
* Delete a quality grade.

The main information in درجه کیفیت form includes:

* کد
* عنوان
* Description related to the quality standard or quality status

درجه کیفیت is different from نوع کالا.

For example, a product can have نوع کالا = «محصول نهایی» while its درجه کیفیت is «درجه ۱» or «معیوب».

**Excel Import and Export Limitations:** Silo currently does not support importing درجه کیفیت کالا records from Excel files or exporting existing درجه کیفیت کالا records to Excel or other file formats.
If the user asks whether this is possible or how to perform it (e.g., "How can I export درجه کیفیت کالا records to Excel?"), clearly state that this functionality is currently not supported.
Do not list sample records, provide alternative data as an export, or imply that the functionality exists.

1. View and Select Existing Records (دکمه طوسی - آیکون لیست): Click this button to display the list of registered درجه کیفیت کالا records. The user can browse the list and select the desired record.
2. Delete Record (دکمه قرمز - آیکون زباله): Click this button to delete the selected درجه کیفیت کالا record.
3. Reset Form (دکمه زرد - آیکون رفرش): Click this button to clear the entered information and reset the form, allowing the user to start entering new information.
4. Save Information (دکمه سرمه ای - آیکون save): Click this button to save the entered information and register the new درجه کیفیت کالا record.

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

# 16. Conceptual Relationship Between Product Information and Operations

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

# 17. Complete Example

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

# 18. Relationship Between گروه کالا and زیرگروه کالا

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

# 19. Relationship Between نوع کالا and Other Information

نوع کالا is a general classification and can be used together with other information to provide a more complete description of a product.

For example:

**نوع کالا:** مواد اولیه
**گروه کالا:** مواد شیمیایی
**برند:** The relevant brand
**سایز:** The specified size
**درجه کیفیت:** درجه ۱

Each of these pieces of information describes a different aspect of the product.

---

# 20. Important Conceptual Differences

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

# 21. Overall Functionality of the Master Information Section

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

# 22. Response Rules for This Knowledge Area

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

# 23. Core Principle of This Knowledge Base

This knowledge base is designed to understand the **user-facing functionality of the Silo system**.

Its primary focus is:

* What information the user manages.
* What each concept means.
* What operations can be performed in each section.
* How different concepts differ from one another.
* How product information is used in business processes.
* How the overall product flow works within warehouses and the factory environment.

Therefore, this knowledge base must be interpreted from a **user-facing and operational perspective**, not from the perspective of technical or internal system structure.

# 24. Synonyms and Semantic Mapping

## Rules
- Map synonyms, alternative names, English terms, and common phrases to the correct canonical concept.
- Use context to resolve ambiguity; ask for clarification when necessary.
- Use canonical Persian names in responses.
- Similar concepts must remain distinct. Synonyms do not imply undocumented features or relationships.

## Concept Mapping
- **گروه کالا:** گروه محصول، دسته کالا، دسته‌بندی کالا، Product Group, Product Category
- **زیرگروه کالا:** زیرگروه محصول، دسته فرعی کالا، Product Subgroup, Product Subcategory
- **نوع کالا:** نوع محصول، Product Type, Item Type
- **طبقه کالا:** طبقه‌بندی کالا، طبقه محصول، Product Class, Product Classification
- **برند:** مارک، نام تجاری، Brand, Product Brand,لوگو
- **سایز:** اندازه، ابعاد (در صورت اشاره به اندازه فیزیکی)، Size, Product Size
- **درجه کیفیت:** کیفیت کالا، وضعیت کیفی، رتبه کیفیت، Quality Grade
- **نوع عملیات:** نوع جابه‌جایی، نوع انتقال کالا، Movement Type, Operation Type

## Important Distinctions
- گروه کالا ≠ زیرگروه کالا ≠ نوع کالا ≠ طبقه کالا
- سایز ≠ واحد کالا ≠ واحد دوم
- درجه کیفیت ≠ نوع کالا
- نوع عملیات، دسته‌بندی کالا نیست.

When a synonym is used, retrieve knowledge associated with its canonical concept and answer based on documented functionality only.
