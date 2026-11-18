# Silo Knowledge Base — رجیستر تگ RFID

## 1. Document Purpose and Scope

This document describes Silo capabilities and concepts related to:

- Serial creation, reservation, and registration (Register)
- Barcode-based and RFID-based identification
- Android registration forms and registration methods
- Multi-tag and Single-tag modes
- Equivalent Tag (تگ معادل) and Double Tag (تگ دوگانه) capabilities
- Serial lifecycle, RFID tag reuse, and operational status
- Freeze, quality control, quantity transfer, and Product placement
- Troubleshooting registration and Product exit issues

This document focuses on capability logic, usage conditions, and operational concepts rather than introducing specific projects or industries.

The availability and behavior of some capabilities may depend on the project, system version, user permissions, and configuration. Such dependencies must be considered when answering users.

---

## 2. Registration, Tagging, and Identification Concepts

### 2.1. Difference Between Tag Reading, Registration, and Warehouse Movement

**Tag Reading (قرائت تگ)** means that a device has read an identifier, such as an EPC.

**Tag Registration (رجیستر تگ)** means that an identification method has been associated with a serial number and that this association has been recorded in the system.

**Warehouse Entry or Movement (ورود یا حرکت انبار)** is a separate event that must be performed according to the relevant workflow.

Therefore:

- Successfully reading a tag does not, by itself, prove that registration has been completed.
- Registering a tag does not necessarily mean that the Product has entered the warehouse.
- Serial issuance, creation of a print record, physical printing, RFID reading, tag registration, and warehouse entry are separate events.

In a typical workflow, the serial and its required information are prepared, the identification value is obtained through a device or another supported method, the corresponding serial is determined, and the association is registered in the system.

The actual steps depend on the form and project.

### 2.2. Barcode and RFID Identification

Registration associates an identification method with a Product serial number. This method can be one of the following:

- **Barcode:** The Product serial number can be used as the barcode identifier. Initially, this number is a text or numeric value and, when required, can be printed as a linear barcode or QR Code.
- **RFID Tag:** An electronic identifier called EPC is assigned to the Product serial number. This identifier is stored on the RFID tag chip and can be read by RFID devices.

Therefore, the term tagging does not necessarily mean using RFID. Registration can be performed using either a barcode or RFID.

In some barcode-based projects, the serial number itself is used as the identification value. This behavior is project-specific and must not be generalized to all RFID implementations.

---

## 3. Serial Creation, Reservation, and Registration

Creating a serial number and registering it are two separate steps.

### 3.1. Serial Creation and Reservation

When a Product record is issued, its serial number and related information are created.

At this stage:

- A serial number has been assigned to the Product.
- Product information, quantity, specifications, and dynamic attributes have been recorded.
- The Product specification sheet and label can be printed.
- **The serial number has not yet entered Silo's operational and Product movement lifecycle.**

This state can be referred to as serial reservation.

### 3.2. Serial Registration and Activation

During the Register stage, the serial number enters the system's operational lifecycle, and its identification method is determined.

At this stage:

- If the registration method is RFID, an EPC identifier is assigned to the Product.
- If the registration method is barcode-based, the serial number itself can be used as the barcode identifier.
- Registration information and its timestamp are stored.
- The serial becomes ready for subsequent operational processes.

**Important Note:** Printing a label or Product specification sheet does not, by itself, mean that the serial has been registered.

### 3.3. Why Is a Serial Created Before the Register Stage?

Silo uses an intermediate stage to create and maintain serial numbers so that their information can be prepared and reviewed before final registration.

This structure is useful in various scenarios:

- When serial numbers have already been prepared in an Excel file.
- When information is received from another system.
- When serial numbers need to be displayed to the operator before final registration.
- When the operator prints labels first and registers them at a later stage.
- When the operator intends to register serials directly without printing them.

One advantage of this approach is maintaining serial number continuity. If a serial is created but, for any reason, is not registered, the next serial is still generated based on the previously reserved number in the standard serial-generation method. This prevents unused serial numbers from being generated again.

This guarantee depends on the serial-generation method and its configuration. If serial generation is dynamic and performed outside the standard reservation mechanism, uniqueness must be guaranteed through another method.

---

## 4. Registration Methods and Android Forms

Registration is performed through various forms on handheld devices and other supported execution platforms.

### 4.1. Android Form «ثبت با کد کالا»

In this form, the operator selects or completes the Product information and required attributes, then reads the tags.

If the serial number does not already exist, the system may generate serials according to the defined formula and perform registration, depending on the form's workflow.

The number of tags read may determine the number of serials required for the request. However, this number alone does not guarantee that all serials have been generated or that all tags have been registered.

The form supports two tag-reading modes:

- **Multi-tag:** Reading multiple tags in a single process.
- **Single-tag:** Restricting the reading process to one tag.

These modes define the tag-reading method. They do not determine the serial-generation method or whether the device is operating online or offline.

### 4.2. Android Form «ثبت با کد کالا و سریال»

This form is used when the serial already exists or is received from an input source.

Depending on the project's rules, the input value may be used unchanged or combined with other information to construct the final serial.

In the described form logic, the request is made for one specific serial. This form is not equivalent to Multi-tag mode in the «ثبت با کد کالا» form.

### 4.3. Bulk Register

In bulk registration, serial information must be recorded in the preparation stage before the Register process begins and must be available in the list of serials ready for registration.

**Printing the serials is not mandatory.** Serial information may have been created in the preparation stage even if its label or specification sheet has not yet been printed.

The process is as follows:

1. The operator opens the bulk registration form on the handheld device.
2. The barcode or serial number of a Product is scanned.
3. The RFID identifier or EPC associated with that Product is also read.
4. The operator repeats this process for other serials and tags.
5. After collecting the required number of records, for example, 50 serials and tags, the operator executes the final registration operation.
6. The system processes the collected information for Register.

Therefore, information for multiple serials and tags is collected first, and final registration is performed afterward.

**Important Note:** The serial information must already exist in the preparation stage, but printing is not a requirement for bulk registration.

### 4.4. Creating a Serial and Performing Register in a Single Process

In some methods, the serial number has not been created in advance.

For example, the operator selects a Product in the «ثبت با کد کالا» form, specifies a quantity of 5, and enters the required dynamic information.

In this case:

1. The system detects that no serial number already exists.
2. Serial numbers are generated according to the defined formula.
3. The initial serial information is prepared.
4. Register is performed immediately afterward.

Therefore, serial creation and registration are two consecutive steps performed as part of a single process from the operator's perspective.

### 4.5. Difference Between Barcode-Based and RFID Registration

In barcode-based registration, after entering or scanning the required information, there is no need to read an RFID tag. Once the operation is finally confirmed, the serial number is considered the barcode identifier.

In RFID registration, the EPC identifier stored on the RFID tag is read and associated with the corresponding serial number.

The selected registration method determines whether the operator must read an RFID identifier or simply confirm the barcode information.

### 4.6. Handheld Devices, Location Information, and Editing Restrictions

In some projects, tag registration or related operations are performed using a **دستگاه هندهلد** (handheld device).

In addition to reading tags, the device may be used to complete information or record location data. For example, a project may require GPS information during registration. This is not a universal requirement and must be verified in the configuration of the relevant project.

Editing serial-specific information, changing an RFID identifier, reprinting, and canceling a tag are different actions.

Whether each action is permitted depends on:

- The record's current status.
- The user's access permissions.
- The project's rules and configuration.

It must not be assumed that every serial or tag can be edited or canceled at every stage.

### 4.7. Online and Offline Considerations

Multi-tag and Single-tag define the tag-reading method, not the device's connection mode.

Whether registration or related operations can be performed offline depends on the supported application workflow and project configuration.

The availability of an offline capability must be verified for the specific form and version. Reading tags offline must not automatically be interpreted as successful registration or synchronization with the central system.

### 4.8. Quality Control and Workflow Completion

Some Products or operations may require quality control.

For example, receiving manufactured goods into the finished-goods warehouse may require quality approval or inspection, while some raw materials may follow a different workflow.

Therefore, quality control must not be assumed to be mandatory for every Product or operation in Silo.

Destination warehouse information, source documents, and quality-control checks are mandatory only when required by the workflow and enabled through the relevant settings.

Tag registration, warehouse operations, and quality approval may be separate stages.

### 4.9. Differences in Capability Availability Across Versions and Projects

Some capabilities, such as registration operations or related gate functionality, were provided as Windows applications in older versions, while some capabilities have been moved to the Web in newer architectures.

Moving a capability to the Web, or its existence in an older version, does not by itself prove that the same capability is available in every newer version or every project.

The availability and behavior of a capability must be verified for the specific version and project in use.

---

## 5. Equivalent Tag (تگ معادل)

Sometimes, a Product needs to be identifiable through more than one physical RFID label.

One way to achieve this is by using the Equivalent Tag feature.

In this method, the first tag has already been registered, and the operator wants the second tag to identify the same Product.

The process is as follows:

1. The operator opens the Equivalent Tag form on the handheld device.
2. The operator reads the first tag, which has already been registered.
3. The Product information and its associated serial number are displayed.
4. The operator presents the second tag.
5. The device writes the EPC identifier of the first tag onto the chip of the second tag.
6. After the operation, both tags have the same EPC identifier.

As a result, if either of these two tags is read by an RFID reader, the system associates it with the same serial number.

**Important Note:** In the Equivalent Tag method, no new serial number is created, and the physical identifier of the second tag is changed to match the first tag's identifier. Therefore, these two tags cannot be distinguished from each other by their EPC identifiers.

### 5.1. When Should Equivalent Tag Be Used?

Equivalent Tag is suitable when the goal is to make two physical labels function exactly like a single identifier.

For example:

- There is a possibility that the original tag may become detached.
- The tag may be exposed to physical damage.
- The Product's geometry makes the tag difficult to read from certain angles.
- The same identifier needs to be readable from multiple locations on the Product.

---

## 6. Double Tag (تگ دوگانه) Capability

Another method of identifying a Product using two RFID tags is available, which differs from Equivalent Tag.

In this method, both tags retain their own independent and different EPC identifiers, but both are associated with the same serial number.

To use this capability, the Double Tag feature must be enabled for the relevant Product.

In this case:

1. The operator starts the Register operation.
2. The system detects that the Product requires two tags.
3. The operator scans the first tag.
4. The operator then scans the second tag.
5. Both identifiers are associated with the same serial number.

From that point onward, reading either tag retrieves the information for the same Product.

### 6.1. Difference Between Equivalent Tag and Double Tag

| Feature | Equivalent Tag | Double Tag |
|---|---|---|
| Number of physical tags | Two or more | Maximum of two tags in the capability described here |
| EPC identifier | Identical | Different |
| Changing the second tag's chip identifier | Yes | No |
| Product serial number | Same | Same |
| Execution method | Equivalent Tag form | During Register |
| Requires enabling a Product feature | No, in the described workflow | Yes |
| Purpose | Making two tags share one identifier | Maintaining two independent identifiers for one Product |

**Note:** Equivalent Tag and Double Tag are two different capabilities and must not be presented as interchangeable.

---

## 7. Serial Lifecycle and Reusing RFID Tags

In Silo, the serial number and RFID tag are two different concepts.

The serial number represents the identity and history of a Product unit, while the RFID tag is an identification tool used to identify that Product.

Some projects use reusable RFID labels.

In these projects:

- An RFID tag is initially assigned to a serial number.
- The Product goes through its operational lifecycle.
- After the lifecycle is completed, the RFID tag is released.
- The same tag can then be assigned to another serial number.

In this situation, the previous serial number is not deleted because its production history, specifications, and operational records must remain traceable.

Only the association between the tag and the previous serial is removed so that the tag can be assigned to a new Product.

Therefore, an empty RFID identifier for a serial does not necessarily mean that the serial has been deleted or was never registered.

### 7.1. Reasons Why a Serial May Have an Empty RFID Identifier

When examining a serial, it is possible to find that no RFID identifier is currently assigned to it.

This situation can have different causes.

#### 7.1.1. Tag Release After Completion of the Operational Lifecycle

In some projects, when a serial reaches a specific stage of its lifecycle, its RFID tag is automatically released.

In this situation:

- The serial number remains.
- The serial's operational history is preserved.
- The association with the RFID identifier is removed.
- The tag can be reused for another serial.

This does not mean that the serial has been deleted.

#### 7.1.2. Operator Error During Bulk Registration

During bulk registration of small Products, several boxes may be placed close to each other.

The operator correctly scans the serial barcode of the second box, but because of the RFID reader's reading range, the device receives the tag identifier from the first box.

If the operator ignores the device warning and confirms the operation, the tag identifier may be detached from the first serial and transferred to the second serial.

As a result:

- The first serial still exists.
- The second serial has also been registered.
- However, one of the serials no longer has its own RFID identifier.

#### Prevention Measures

- Maintain an appropriate distance between Products during bulk registration.
- Adjust the RFID reading range according to the dimensions of the Products and the environmental conditions.
- Operators must pay attention to device warnings.
- Before final confirmation, ensure that the barcode and RFID tag correspond to the same Product.

---

## 8. Serial Status in the Operational Lifecycle

Each serial in Silo is assigned to a specific operational destination.

The destination can represent different stages of the workflow, for example:

- صدور شناسنامه
- در حال بازسازی
- اتمام بازسازی
- تحویل به مشتری

After each successful movement, the serial's current destination changes, and its status is updated according to the type of the new destination.

Therefore, serial status is not merely a general state such as active or inactive; it can indicate the Product's current operational stage.

---

## 9. Freezing a Serial

The Freeze capability is used to temporarily block a single serial or a group of serials.

This capability is commonly used in quality control, inspection, or management of Products with identified issues.

The general process is as follows:

1. A serial or a group of serials is selected.
2. The reason or description for the restriction is recorded.
3. The serials are placed in a frozen state.
4. If the Freeze control is enabled for an operation, the system does not allow a frozen serial to be moved.
5. After the issue has been investigated and resolved, the serial can be unfrozen.

**Important Note:** A frozen serial is not necessarily prohibited from participating in every operation in the system. Its impact depends on the control settings of the relevant Action Type (نوع عملیات).

---

## 10. Quality Control and Product Rejection

In Silo, information related to the quality assessment of a serial can be recorded through inspection processes.

If a Product fails a quality assessment, it may be placed in a locked state (قفل).

In this situation, subsequent operations may enforce controls related to the failed quality assessment.

For example, if the QC faild control is enabled for the "ارسال به فروش" operation, a rejected serial will not be allowed to undergo this operation.

A serial may also be inspected multiple times. In that case, the latest inspection result is available for use in operational controls.

Quality control is not necessarily mandatory for every Product or operation. Its requirements depend on the relevant workflow and configuration.

---

## 11. Transferring Part of a Serial's Quantity to Another Serial

Silo allows part of the quantity associated with one serial to be transferred to another serial.

For example:

- A package contains 40 items.
- The operator intends to separate 10 items from it.
- This quantity can be transferred to a new serial or an existing serial.

Another example is a tile pallet containing 80 square meters, from which 40 square meters are transferred to another serial.

In this situation, the system can preserve the relationship between the source serial and the destination serial so that the history of the quantity separation and transfer remains traceable.

This capability is different from packaging levels and parent-child relationships between serials.

---

## 12. Product Placement

In Silo, Product placement can be performed at the serial level.

This means that the system determines the physical location or Location assigned to each serial number.

This information is used in warehouse operations, Product locating, picking, and inventory control.

Location information may also be collected through a handheld device in projects that support this capability. Any additional requirements, such as GPS information, depend on the relevant project configuration.

---

## 13. Troubleshooting Product Exit Errors

When a user reports that "the system does not allow the tag to exit," the first step is to determine which operation they mean.

The term "exit" alone is not sufficient to identify the cause of the error because every movement in Silo is performed through a specific Action Type (نوع عملیات).

For example, the user may be referring to the "ارسال به فروش" operation.

After identifying the operation, the following items must be checked:

### 13.1. Product Presence in the Source Warehouse

Is the serial located in the source warehouse required by the operation?

If the source-presence control is enabled and the Product is located in another warehouse, the operation will not be completed.

**Solution:**

- First, move the Product to an authorized source warehouse.
- Alternatively, if approved, the system administrator can review and modify the relevant control in the operation settings.

### 13.2. Product Freeze Status

If the Freeze control is enabled for the operation, a frozen serial cannot be moved.

**Solution:**

- Investigate the reason for the Freeze.
- If the issue has been resolved, unfreeze the serial.
- If necessary, have the system administrator review the operation control settings.

### 13.3. Quality Control Status

The Product may not have received QC approval yet, or it may have failed its latest inspection.

If the relevant QC control is enabled for the operation, the system will not allow the Product to exit.

**Solution:**

- Check the inspection status.
- Complete the quality-control process.
- If the Product is approved, record the appropriate result.

### 13.4. Other Operational Controls

Each Action Type can have its own set of independent controls.

Therefore, the same cause must not be assumed for every exit error.

The actual cause must be determined from the error message, the serial's status, and the active controls of the relevant Action Type.

### 13.5. Information Required for Troubleshooting

The following information is important for accurately investigating the issue:

- Product serial number
- Product name or code
- Exact name of the intended operation
- Source and destination warehouses
- Exact error message displayed
- Product Freeze and quality-control status

---

## 14. Inference Limitations and Rules for Preventing Incorrect Answers

The agent must follow these rules when answering questions about registration, tagging, and serial lifecycle management:

1. **Distinguish separate events:** Serial issuance, creation of a print record, physical printing, RFID reading, tag registration, and warehouse entry are separate events. Completion must be reported only when the result of the relevant operation has been verified.

2. **Distinguish registration forms:** Do not treat «ثبت با کد کالا» and «ثبت با کد کالا و سریال» as equivalent forms.

3. **Distinguish reading modes:** Multi-tag and Single-tag define tag-reading behavior. They must not be interpreted as serial-generation methods or online/offline modes.

4. **Distinguish identification methods:** Barcode-based registration and RFID registration are different methods. Do not assume that every registration process requires reading an RFID tag.

5. **Distinguish Equivalent Tag and Double Tag:** Equivalent Tag uses the same EPC identifier on multiple physical tags, whereas Double Tag associates two different EPC identifiers with one serial number.

6. **Do not confuse registration with warehouse movement:** Successful registration does not prove that the Product has entered a warehouse or completed a warehouse operation.

7. **Respect project-specific behavior:** Requirements such as GPS collection, quality control, destination warehouse information, and source documents must not be treated as universal requirements.

8. **Respect permissions and record status:** Do not assume that every serial or tag can be edited, reprinted, changed, or canceled at every stage.

9. **Verify version and configuration:** If behavior depends on the project, version, permissions, or configuration and reliable information is unavailable, state this limitation explicitly. Do not fill in missing details by guessing.

10. **Do not assume offline completion:** Reading or collecting tag information offline does not, by itself, prove that registration has been completed or synchronized with the central system.

11. **Do not infer the cause of an exit error:** If the Action Type is not specified, first ask the user for the exact operation name. If the cause is unclear, investigate the serial's actual status, the error message, and the active controls of that operation.

12. **Report only verified results:** Do not state that an operation succeeded unless its completion has been confirmed through reliable system information.

---

## 15. Summary of Capabilities

When answering users, the agent must distinguish between:

- An existing system capability.
- A capability whose availability or behavior depends on configuration.
- A technical or operational limitation.
- A capability that is planned for a future enhancement but has not yet been implemented.

Details that have not been confirmed by the available knowledge must not be stated as facts.

The agent must explain the difference between tag reading, registration, and warehouse movement, identify the relevant registration method and form, and consider the project's workflow and configuration before describing system behavior.