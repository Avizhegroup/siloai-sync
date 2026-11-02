# Silo Knowledge Base — رجیستر تگ RFID

## 1. Document Purpose and Scope

This document describes Silo capabilities in the following areas:

- رجیستر تگ RFID and its distinction from tag reading
- Android forms, Multi-tag and Single-tag modes, and offline considerations

This document focuses on capability logic and usage conditions rather than introducing implemented projects or specific industries.

---

## 2. رجیستر تگ RFID and Android Forms

### 2.1. Difference Between Tag Reading, Registration, and Warehouse Entry

**Tag Reading (قرائت تگ)** means that a device has read an identifier such as an EPC.

**Tag Registration (رجیستر تگ)** means that an RFID identifier has been associated with its corresponding serial and that this association has been recorded in the system.

**Warehouse Entry or Movement (ورود یا حرکت انبار)** is a separate event that must be performed according to the relevant workflow.

Therefore, successfully reading a tag does not, by itself, prove that tag registration has been completed. Likewise, tag registration does not necessarily mean that the product has entered the warehouse.

In a typical workflow, the serial and required information are prepared, the tag identifier is obtained through a device or a supported method, the corresponding serial is determined, and the association between the tag identifier and the serial is then registered.

The actual steps depend on the form and project. In some barcode-based projects, the serial number may be used as the tag identifier. This behavior is project-specific and must not be generalized to all RFID implementations.

### 2.2. Android Form «ثبت با کد کالا»

In the described scenario, the user selects or completes the product and required information and then reads the tags.

The number of tags read may determine the number of serials required for the request. However, this number alone does not guarantee that all serials have been generated or that all tags have been registered.

- **Multi-tag:** Reading multiple tags in a single process.
- **Single-tag:** Restricting the reading process to one tag.

These modes define the tag-reading method, not the serial-generation method or whether the device is operating online or offline.

### 2.3. Android Form «ثبت با کد کالا و سریال»

This form is used when the serial already exists or is received from an input source.

Depending on the project's rules, the input value may be used unchanged or combined with other information to construct the final serial.

In the described form logic, the request is made for one specific serial. This form is not equivalent to Multi-tag mode in the «ثبت با کد کالا» form.

### 2.4. Registration Using a دستگاه هندهلد, Location Information, and Editing Restrictions

In some projects, tag registration or related operations are performed using a **دستگاه هندهلد** (handheld device).

In addition to reading tags, the device may be used to complete information or record location data. For example, a project may require GPS information during registration. This is not a universal requirement and must be verified in the configuration of the relevant project.

Editing serial-specific information, changing an RFID identifier, reprinting, and canceling a tag are different actions. Whether each action is permitted depends on the record's status, the user's access permissions, and the project's rules.

It must not be assumed that every serial or tag can be edited or canceled at every stage.

### 2.5. Quality Control and Workflow Completion

Some products or operations may require quality control. For example, receiving manufactured goods into the finished-goods warehouse may require quality approval or inspection, while some raw materials may follow a different workflow.

Therefore, quality control must not be assumed to be mandatory for every product or operation in Silo.

Destination warehouse information, source documents, and quality-control checks are mandatory only when required by the workflow and enabled through the relevant settings.

Tag registration, warehouse operations, and quality approval may be separate stages.

### 2.6. Differences in Capability Availability Across Versions and Projects

Some capabilities, such as registration operations or related gate functionality, were provided as Windows applications in older versions, while some capabilities have been moved to the Web in newer architectures.

Moving a capability to the Web, or its existence in an older version, does not by itself prove that the same capability is available in every newer version or every project.

The availability and behavior of a capability must be verified for the specific version and project in use.

---

## 3. Inference Limitations and Rules for Preventing Incorrect Answers

15. Serial issuance, creation of a print record, physical printing, RFID reading, tag registration, and warehouse entry are separate events. Completion must be reported only when the result of the relevant operation has been verified.

17. Distinguish between the «ثبت با کد کالا» and «ثبت با کد کالا و سریال» forms, as well as between Multi-tag and Single-tag modes.

21. If behavior depends on the project, version, permissions, or configuration and reliable information is unavailable, state this limitation explicitly. Do not fill in missing details by guessing.

---

## 4. Summary of Capabilities

When answering users, the agent must always distinguish between an existing capability, a configuration-dependent capability, a technical limitation, and a planned future enhancement. Details that have not been confirmed by the available knowledge must not be stated as facts.