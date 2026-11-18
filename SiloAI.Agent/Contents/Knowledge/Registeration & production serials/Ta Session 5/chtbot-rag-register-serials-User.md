# Silo User Guide: صدور سریال, چاپ لیبل, and رجیستر تگ RFID

## 1. Purpose and Scope of the Guide

This guide explains how to create product identity records (شناسنامه کالا), generate or obtain serial numbers (صدور یا تأمین شماره سریال), complete product information, print labels (چاپ لیبل), and register RFID tags (رجیستر تگ RFID) in the Silo system.

The guide addresses common user questions about the following topics:

- روش‌های صدور یا تأمین شماره سریال.
- صدور تکی و گروهی سریال.
- Required information when generating a serial number.
- Dynamic fields and product-specific information (اطلاعات اختصاصی محصول).
- Label printing and reprinting .
- رجیستر تگ RFID.
- Differences between Android forms.
- Differences between online and offline versions.
- Warehouse information, documents, and quality control.
- Editing information and access restrictions.

Available features may vary depending on the project, software version, and system configuration. This guide therefore describes the general process. Project-specific details must be verified against the actual configuration.

## 2. Difference Between Serial Number Generation, Label Printing, and Tag Registration

Three separate activities are involved in creating an identity record for a product.

**Serial Number Generation (صدور سریال):** Creating or obtaining a number used to identify a specific product unit, package, or quantity.

**Label Printing (چاپ لیبل):** Printing the serial number, barcode, or other required information on a label.

**RFID Tag Registration (رجیستر تگ RFID):** Associating an RFID tag identifier with the corresponding serial number and registering that association in the system.

These steps may be performed as part of one continuous workflow or at different times.

For example, a product's serial number may be generated and its label printed, while the RFID tag is attached and registered later.

Likewise, successfully reading an RFID tag does not, by itself, prove that registration has been completed. The operation result must be checked to confirm successful registration.

## 3. Methods of Generating or Obtaining Serial Numbers

The method used to generate or obtain a serial number depends on project requirements and system configuration.

### 3.1. Sequential Numeric Serial Numbers (سریال عددی متوالی)

In this method, serial numbers are generated according to a numeric sequence.

For example:

`1000, 1001, 1002, 1003`

Under certain circumstances, gaps may occur between numbers. Therefore, uninterrupted numbering must not be assumed without verification.

### 3.2. Automatically Generated Serial Numbers (سریال مبتنی بر شناسه خودکار)

In some projects, the system uses an automatically generated identifier to create or obtain serial numbers.

This method can be suitable for assigning numbers, but it does not necessarily guarantee uninterrupted sequential numbering.

### 3.3. Serial Numbers Generated Using a Custom Formula (سریال با فرمول اختصاصی)

In some projects, serial numbers are generated according to a defined pattern.

For example, a pattern may combine a product code, a date, and a counter.

The actual formula depends on the project. Serial number formats and their components are therefore not necessarily the same across all Silo projects.

### 3.4. Using an Existing Serial Number (استفاده از سریال موجود)

Sometimes, a product already has a serial number. For example, certain devices have an IMEI number.

Depending on the project's rules, the system may use that number directly or combine it with other information to produce the final serial number.

**Important:** The presence of an input number does not guarantee that the same number will be registered as the final serial number.

## 4. Serial Number Generation by Quantity

Serial numbers may be generated individually or in batches.

### 4.1. Individual Serial Number Generation (صدور تکی)

In this method, a serial number is requested for one specific product unit or identity record.

This method may be useful in projects where each unit requires extensive individual information. For example, in automotive projects, each vehicle may have its own chassis number, VIN, or license plate.

### 4.2. Batch Serial Number Generation (صدور گروهی)

In this method, the user requests a specified number of serial numbers for a product.

For example, a user may request 20 serial numbers for one product.

This approach may be useful in projects where general product information has already been defined and less individual information needs to be entered for each unit.

Support for individual or batch generation, as well as the maximum number of serial numbers allowed, depends on the form and project configuration.

## 5. Where Can Serial Numbers Be Generated?

Depending on the features enabled in a Silo project, serial numbers may be generated or obtained through the following platforms:

- نرم‌افزار تحت وب.
- نرم‌افزار اندروید آنلاین.
- نرم‌افزار اندروید آفلاین.
- ‌افزارهای ویندوزی یا کیوسک.
- Integration with other systems through an API (ارتباط از طریق API).

The serial number generation method and the platform used to execute it are two separate matters. For example, a serial number may be generated using a custom formula through the web application, or it may be obtained from another system and then used in Silo.

A feature available in one version is not necessarily enabled in every version or project.

## 6. Generating Serial Numbers Through the Web Application

### 6.1. Form Location (مسیر فرم)

In the configuration described here, the serial number generation form is located under:

`Print / Management`

The form title or menu path may differ depending on the software version and project configuration.

If the form does not appear in the user's menu, the menu structure or access permissions may be responsible. The absence of a visible form does not, by itself, prove that the feature is unavailable in the system.

### 6.2. General Steps for Serial Number Generation 

**Step 1: Select the Product**

Select the required product from the list of defined products. General product information may be displayed according to the form configuration.

**Step 2: Enter Production Information**

Depending on the project, enter or select information such as the production line, production hall, work shift (شیفت کاری), or other production details.

**Step 3: Select the Destination Warehouse**

If the form requires destination information for recording the product's warehouse entry (ورود به انبار), select the appropriate warehouse.

Whether a warehouse must be selected, and at which stage it is required, depends on the project's operational workflow.

**Step 4: Complete Document Information**

Some operations require a document number or related document information. If the relevant controls are enabled, the system may compare the request with the document's permitted items and quantities.

**Step 5: Specify the Product Quantity**

The quantity specifies how much product each serial number represents.

**Step 6: Specify the Number of Serial Numbers**

In forms that support batch generation, specify the required number of serial numbers. In some forms or scenarios, the number may be predefined or determined by the workflow.

**Step 7: Complete Product-Specific Information**

Depending on the product type and project configuration, additional information may be required, such as technical specifications, an order number, or information specific to an individual unit.

Some fields may be editable, while others may be populated automatically.

**Step 8: Submit the Request**

After completing the required information, submit the request and check the result reported by the system.

Submitting a request does not, by itself, prove that all requested serial numbers were successfully generated.

## 7. Dynamic Fields and Product-Specific Information

Some Silo forms use dynamic fields (فیلدهای پویا) to collect project-specific information. These fields allow the system to request information appropriate to the product or operation.

Dynamic fields may be used in three areas.

### 7.1. Dynamic Product Information

This information relates to the general characteristics of a product.

For example, a project may record the following product attributes:

- عرض
- گرماژ
- نوع سوخت
- نوع کاربری

The required fields and values depend on the product type and project configuration.

### 7.2. Serial-Specific Information 

This information belongs to a specific product unit or identity record.

For example, an automotive project may record the following information for each serial number:

- پلاک انتظامی
- شماره شاسی
- شماره بدنه
- شماره VIN

Two units of the same product may share identical general product information while having different unit-specific information.

If the form supports it, serial-specific information may be entered during serial number generation or completed in a separate step.

### 7.3. Dynamic Operation Information 

Some information is required only when a particular operation is performed.

For example, during a production operation, the user may be asked to specify the device type or the method used to execute the operation.

This information relates to the operation itself and is not necessarily part of the product's general specifications.

### 7.4. Field Types 

Depending on the form design, fields may be:

- Editable by the user (قابل ویرایش).
- Read-only and used only to display information (فقط نمایشی).
- Populated automatically (تکمیل خودکار).
- Calculated from other information (محاسباتی).

The actual behavior of each field must be verified in the relevant form and project.

## 8. Product Quantity and Default Quantity (مقدار کالا و مقدار پیش‌فرض)

Some projects define a default quantity for a product. However, if the form supports it, the quantity associated with a particular serial number may be adjustable.

For example, if the default quantity for one pallet of tiles is 60 square meters, a permitted scenario might allow the quantity for a specific pallet to be changed to 30 square meters.

This example does not mean that quantity editing is available in every project. The ability to change the quantity depends on the form configuration and applicable restrictions.

### Difference Between the Number of Serial Numbers and the Quantity per Serial Number

- **Number of serial numbers (تعداد سریال):** The number of identity records to be created.
- **Quantity per serial number (مقدار هر سریال):** The quantity of product represented by each identity record.

For example, five serial numbers representing 87 pieces each are not equivalent to one serial number representing 435 pieces, even though the total product quantity is the same.

## 9. Label Printing and Reprinting

After a serial number has been generated, a label can be printed if the form supports printing and an appropriate print template is available.

Depending on project requirements, the print template may contain different types of information, such as:

- Serial number or barcode (شماره سریال یا بارکد).
- General product information (مشخصات عمومی محصول).
- Product-specific information required to identify the item (اطلاعات اختصاصی کالا).

The system may also support reprinting (چاپ مجدد).

Reprinting does not necessarily generate a new serial number. The same existing serial number may simply be printed again.

Likewise, the existence of a serial number does not prove that its label has been printed. Printing status must be verified using the operation status and the actual capabilities of the system.

## 10. RFID Tag Registration (رجیستر تگ RFID)

Tag registration is the process of associating an RFID identifier with the appropriate serial number and recording that association in the system.

In a typical scenario, the steps are as follows:

1. The serial number and required information are available.
2. The RFID tag identifier is obtained through a supported device or method.
3. The system identifies the corresponding serial number.
4. The RFID identifier is associated with the relevant serial number.
5. The tag registration operation is performed.

The exact steps may vary between projects.

### 10.1. Difference Between Reading and Registration (تفاوت قرائت و رجیستر)

Reading an RFID identifier only indicates that the identifier has been received. It does not, by itself, prove that the tag has been successfully registered.

The operation result must be checked to confirm that registration is complete.

### 10.2. Barcode-Based Projects 

In some projects, the registration method differs from the typical RFID workflow. The serial number itself may be used as the tag identifier.

This behavior depends on the design and configuration of the specific project and must not be generalized to all Silo projects.

### 10.3. Tag Registration and Warehouse Entry

Tag registration and completion of the warehouse-entry process (فرایند ورود کالا به انبار) are not necessarily the same thing.

In some workflows, after registration, product movement or destination information must also be recorded. To determine whether the operation is complete, identify all the steps required by the relevant workflow.

## 11. Serial Number Generation on Android 

This guide distinguishes between two Android forms:

1. Registration by Product Code (ثبت با کد کالا).
2. Registration by Product Code and Serial Number (ثبت با کد کالا و سریال).

The exact capabilities of each form depend on the software version and project configuration.

### 11.1. “Registration by Product Code” Form (فرم ثبت با کد کالا)

In this form, the user first selects the product and enters the required information, then reads the RFID tags.

In the scenario described here, the number of tags read determines the number of serial numbers required for the current request.

For example, if six tags are read during the current process, the request will require six serial numbers.

This count defines the number of serial numbers required by the request. It does not necessarily mean that all serial numbers will be generated successfully or that all tags will be registered.

**حالت Multi-tag**

This mode supports reading multiple tags during a single process.

The number of tags read can determine the number of serial numbers required by the request.

**حالت Single-tag**

This mode limits the reading process to one tag.

Multi-tag and Single-tag describe how tags are read. They must not be confused with the serial number generation method or with whether the device operates online or offline.

### 11.2. “Registration by Product Code and Serial Number” Form (فرم ثبت با کد کالا و سریال)

This form is used when the product's serial number already exists or is obtained from the product or another input source.

Depending on the project's rules, the input number may be used as-is, or the final serial number may be created by combining it with other information.

In the form logic described here, the request is made for one specific serial number and is therefore a single-item request in terms of quantity.

This form is not equivalent to Multi-tag mode in the “Registration by Product Code” form.

## 12. Differences Between Online and Offline Android (اندروید آنلاین و آفلاین)

### 12.1. Online Version 

In online mode, the device is connected to the server and sends the request for processing.

The result must be verified using the system response and recorded operation status. A server connection alone does not prove that the operation succeeded.

### 12.2. Offline Version

In offline mode, some operations may initially be performed using local device data and may not yet be registered on the central server.

After connectivity is restored, the data is transferred according to the system's synchronization mechanism (همگام‌سازی).

Some controls available in the online version may not be available offline because they require access to central data or communication with the server.

To verify the status of an offline operation, determine:

- Whether the operation was performed only on the device or was also transferred to the server.
- Whether synchronization has completed.
- What final processing result was returned by the server.

The exact behavior depends on the implementation of the specific version.

## 13. Document Information and Operational Controls 

In some projects, entering a document number is mandatory when generating serial numbers or performing an operation.

Depending on the project configuration, the system may:

1. Receive and validate the document number.
2. Retrieve the document items from the relevant source.
3. Compare the requested product with the document items.
4. Compare the requested quantity with the quantity permitted by the document.

For example, if the requested product is not included in the document or the requested quantity exceeds the permitted quantity, the system may prevent the operation from continuing.

In some projects, document information is retrieved from an external system.

These controls are not identical across all operations. Whether each control is enabled and how it is executed depend on the actual project configuration.

## 14. Quality Control (کنترل کیفیت)

Some products or operations may require quality control.

For example, receiving a finished product into the finished-goods warehouse may require quality approval or inspection, while certain raw materials may follow a different workflow.

Consequently, the conditions for registering and receiving products may vary according to the product type and operation type.

Do not assume that quality control is mandatory for every product or every Silo operation.

## 15. Registration Using a هندهلد Device 

In some projects, tag registration is performed using a handheld device (دستگاه هندهلد).

Depending on project requirements, the device may be used not only to read RFID tags but also to complete information or record a location.

For example, in an automotive project, the device's GPS information may be required during registration.

This is not a universal feature across all projects. The requirement to record a location depends on the configuration and requirements of the specific project.

## 16. Editing and Cancelling Information (ویرایش و ابطال اطلاعات)

The ability to edit or cancel information depends on the record's status, the user's permissions, and the project's rules.

Do not assume that every serial number or tag can be edited or cancelled at every stage.

Editing serial-specific information, changing an RFID identifier, reprinting a label, and cancelling a tag may be separate activities.

To determine whether a particular activity is permitted, check the actual capabilities of the form and the current status of the record.

## 17. Web Version and Windows Applications 

Some Silo capabilities, such as registration operations or Gate functionality, were provided as Windows applications in older versions, while some capabilities have been moved to the web application in newer architectures.

The web version can support centralized software change management and reduce the need to update each user's system separately.

However, the availability of a feature in the web or Windows version must be verified for the specific project. Do not assume that every feature available in older versions also exists in every newer version.

## 18. Frequently Asked Questions 

**Does generating a serial number mean that a label has been printed?**

No. Serial number generation  and label printing are separate activities. Printing must be performed through a supported workflow.

**Does the existence of a serial number mean that the tag has been registered?**

No. The existence of a serial number alone does not prove that the tag has been registered successfully.

**Does successfully reading an RFID tag mean registration is complete?**

No. Reading only confirms that the identifier was received. The tag registration result must be checked.

**Is the serial number generation method the same in every project?**

No. A project may use sequential numbering, an automatic identifier, a custom formula, or an existing serial number.

**Can an existing product serial number be used?**

In some projects, yes. Whether the input number is used unchanged or modified depends on the rules of the specific project.

**Can multiple serial numbers be generated for one product?**

Yes, in forms and projects that support batch generation (صدور گروهی). The number that can be requested depends on form limitations and project configuration.

**Are the number of serial numbers and the product quantity the same?**

No. The number of serial numbers specifies how many identity records will be created. The quantity per serial number specifies how much product each record represents.

**What are dynamic fields used for?**

Dynamic fields collect information appropriate to a product, a specific serial number, or an operation. The required fields depend on the project configuration.

**Does reprinting create a new serial number?**

Not necessarily. Reprinting may print the same existing serial number again.

**Why can't I enter the number of serial numbers manually in the Android form?**

In the “Registration by Product Code” form (ثبت با کد کالا), the number of tags read can determine the number of serial numbers required by the request.

**Does Android offline have all the capabilities of the online version?**

Not necessarily. Some features and controls depend on server connectivity or access to central data.

**Why is the form not visible in my menu?**

The menu structure or access permissions may prevent the form from appearing. To identify the cause, check the settings for the relevant user and project, and contact support if necessary.

**Can a serial number always be edited after it has been generated?**

No. Editing depends on the record's status, user permissions, and the project's operational rules.

**Does the product always enter the warehouse after tag registration?**

This depends on the project's workflow. In some workflows, recording product movement and the warehouse destination is also required. The results of the relevant steps must be checked to confirm completion.

## 19. Rules for the Agent When Answering Users 

When answering user questions about serial number generation, label printing, and tag registration, the agent must:

1. Use clear, practical operational terminology.
2. Avoid exposing internal database details in ordinary user-facing answers.
3. Clearly distinguish serial number generation, label printing, RFID reading, and tag registration.
4. Distinguish the number of serial numbers from the quantity per serial number.
5. Correctly explain the differences between Android forms and Multi-tag and Single-tag modes.
6. Never generalize a project-specific serial number formula to other projects.
7. Never assume that online and offline versions behave identically without verification.
8. Report successful completion only when supported by a valid operation result.
9. If a feature or form behavior depends on the project and the exact information is unavailable, explicitly state this limitation.
10. Avoid describing steps or capabilities that have not been confirmed to exist in the relevant project.

## 20. Summary 

The product identification workflow in Silo may include serial number generation , product information completion, label printing , RFID reading , tag registration , and recording warehouse information.

These steps are related but are not interchangeable. The serial number generation method, number of identity records, quantity per serial number, dynamic fields, and execution platform also depend on project requirements and system configuration.

To answer user questions accurately, the agent must consider the actual capabilities of the relevant form and project. It must not infer that printing, tag registration, or warehouse entry has been completed merely because a serial number exists or an RFID tag has been read.
