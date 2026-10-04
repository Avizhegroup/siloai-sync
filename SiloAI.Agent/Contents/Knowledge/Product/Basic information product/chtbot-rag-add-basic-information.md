# Comprehensive and Operational Silo System Knowledge Base: Master Data and Factory Warehouse Flow

## 1. Introduction and System Philosophy

This document is the official reference and knowledge base for the intelligent agent when answering user questions about product master data, physical specifications, quality classification, and warehouse and production flows in the Silo software.

The agent must use this document to guide users on how to register, manage, and analyze master data related to warehouses and factory operations.

---

## 2. Master Data Structure and Product Taxonomy

### 2.1. Product Type Specification Form (`/product/addproducttype`)

**Purpose and Usage:**
Product Type (نوع کالا) is the highest-level classification in the item definition structure. It determines the general nature of a product in the system, such as raw materials (مواد اولیه), finished products (محصول نهایی), spare parts (قطعات یدکی), or consumable items (اقلام مصرفی).

**Field Descriptions:**

* **Product Type Code (کد نوع کالا):** A unique numeric identifier used to distinguish product types at the system level.
* **Product Type Title (عنوان نوع کالا):** A fully descriptive name that represents the nature of the product category, such as «مواد اولیه فلزی» or «محصولات کارخانه».

---

### 2.2. Product Class Form (`/product/class`)

**Purpose and Usage:**
Product Class (طبقه کالا) is an independent structural or technical classification layer used to organize items based on their structural characteristics.

**Field Descriptions:**

* **Code (کد):** System identifier of the product class.
* **Title (عنوان):** Main name of the product class.
* **Subtitle (عنوان فرعی):** A subcategory or supplementary title used to distinguish between similar classes more precisely.
* **Description (توضیحات):** Text used to describe the purpose or application of the class within the factory structure.

---

### 2.3. Product Group and Product Subgroup Forms (`/product/group` and `/product/subGroup`)

**Purpose and Usage:**
The Silo system uses a hierarchical parent-child structure (ساختار درختواره‌ای پدر-فرزندی) for product classification.

**Product Group (گروه کالا)** represents the main category, while **Product Subgroup (زیرگروه کالا)** provides more detailed classification within that group.

**Product Group (`group`) Fields:**

* **Code and Title (کد و عنوان):** Identifier and name of the main product group.

**Product Subgroup (`subGroup`) Fields:**

* **Code, Title, and Subtitle (کد، عنوان و عنوان فرعی):** Identification information for the subgroup.
* **Description (توضیحات):** Additional notes or information.
* **Product Group Selection (انتخاب گروه کالا):** A critical relationship field that determines which Product Group this subgroup belongs to. Without defining this relationship, the subgroup has no structural meaning within the product hierarchy.

---

### 2.4. Product Brand Form (`/product/brand`)

**Purpose and Usage:**
The Brand (برند کالا) identifies the manufacturer, commercial name, or brand of a product and is used in warehouse reports and business processes.

**Field Descriptions:**

* **Code (کد):** Unique identifier of the brand.
* **Title (عنوان):** Registered commercial brand name, such as the name of a domestic or international manufacturer.

---

### 2.5. Product Size and Physical Specifications Form (`/product/size`)

**Purpose and Usage:**
The Product Size (سایز) form goes beyond simple dimensions. It defines measurement units, packaging quantities, weight, and shipment volume for accurate inventory and warehouse management.

**Field Descriptions:**

* **Code and Title (کد و عنوان):** Identification information for the size.
* **Description and Other Information (توضیحات و سایر اطلاعات):** Technical dimensional details or operator notes.
* **Product Unit (واحد کالا):** The primary unit of measurement for the product, such as piece (عدد), board (تخته), or roll (رول).
* **Second Unit (واحد دوم):** A supplementary unit of measurement, such as square meter (متر مربع), weight, or length, used for products that are measured using two units.
* **Second Unit Quantity per Shipment (مقدار واحد دوم در محموله):** The conversion ratio or amount of the second unit associated with one package or shipment.
* **Shipment Quantity (مقدار محموله):** The total quantity of the product in the packaging unit or shipment.
* **Shipment Weight and Shipment Volume (وزن محموله و حجم محموله):** Physical weight and volume values that are important for transportation and warehouse calculations.

---

### 2.6. Product Quality Grade Form (`/product/qc`)

**Purpose and Usage:**
In factory and production environments, distinguishing product quality levels is highly important. The Product Quality Grade (درجه کیفیت کالا) form is used to classify product quality, such as first-grade products (محصول درجه یک), second-grade products (محصول درجه دو), defective products (محصول معیوب), or scrap (ضایعات).

**Field Descriptions:**

* **Code and Title (کد و عنوان):** Identifier of the quality grade, such as «کیفیت الف» or «ضایعات خط تولید».
* **Description (توضیحات):** Quality standards or specifications associated with the grade.

---

## 3. Factory Operations and Warehouse Flow (`/product/addactiontype` — Add Operation Type Form)

### 3.1. General Concept of "Operation Type" in Warehouse Management

**Definition:**
In the Silo system, an Operation Type (نوع عملیات) is not merely a name. It is a set of business rules, source, destination, and control mechanisms that determine how an item or shipment moves between warehouses.

**Factory Scenario:**
For example, transferring material from the «انبار مواد اولیه» (Raw Material Warehouse) to the «خط تولید» (Production Line), or from the «خط تولید» to the «انبار محصول نهایی» (Finished Product Warehouse), requires a dedicated Operation Type for each flow.

---

### 3.2. Detailed Fields of the Add Operation Type Form

* **Code and Title (کد و عنوان):** Identifier and name of the warehouse operation, such as «حواله مصرف مواد» (Material Consumption Issue) or «رسید تولید محصول» (Production Receipt).
* **Source Warehouse Type (نوع انبار مبدا):** Determines the type of warehouse from which the product or shipment is moved at the beginning of the operation, such as a quarantine warehouse (انبار قرنطینه) or raw material warehouse (انبار مواد).
* **Target Warehouse Type (نوع انبار مقصد):** Determines the destination of the movement, such as a sold-product warehouse (انبار فروش‌رفته) or scrap warehouse (انبار ضایعات).
* **RFID Power (قدرت Rfid):** Determines the level of access or processing capability of RFID hardware, such as tag readers and automatic registration stations, for the specific operation.
* **Allowed Document Status and Document Change (وضعیت سند مجاز و تغییر سند):** Determines which status lifecycle the warehouse-related document must go through, such as changing from an unregistered state to an approved or submitted state.
* **Active Operational Controls (کنترل‌های عملیاتی فعال):** Restrictions and validation rules applied by the system during the operation, such as location validation, document matching, or mandatory tag scanning.

---

## 4. Agent Behavior and Response Rules

### Conceptual Separation

The agent must clearly distinguish between **Master Data (اطلاعات پایه)** and **Operation Type (نوع عملیات)**.

Master data such as Product Type (نوع کالا), Size (سایز), Brand (برند), Product Group (گروه کالا), and Quality Grade (درجه کیفیت) are prerequisites for registering and tracking products in the system.

In contrast, **Operation Type (نوع عملیات)** controls the movement and flow of these products between warehouses and factory locations.

### Avoid Guessing

If a user asks about the existence of a specific Operation Type, Brand, Product Group, or other master-data item and the requested information is not available in the documented data, the agent must not provide a speculative answer.

Instead, the agent should clearly state that sufficient documented information is not available.

### Response Language and Structure

Responses must be fully operational, clear, and written in fluent Persian so that factory operators and warehouse managers can easily understand the system logic and perform the required operations.
