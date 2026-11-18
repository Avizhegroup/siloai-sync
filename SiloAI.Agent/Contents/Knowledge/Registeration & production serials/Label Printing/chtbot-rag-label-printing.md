# Silo Knowledge Base — چاپ لیبل و قالب‌های چاپ

## 1. Document Purpose and Scope

This document describes Silo capabilities in the following areas:

- Initial printing, reprinting, and print template selection
- Support for FastReport, ZPL, and ZMin templates
- Printer selection, print template file storage, and filtering of relevant templates

This document focuses on capability logic and usage conditions, rather than introducing implemented projects or specific industries.

---

## 2. Initial Printing and Reprinting

### 2.1. Initial Printing

Initial printing is part of the serial issuance workflow. During this process, the serial and its associated information are sent to an appropriate template for printing.

In the described architecture, initial printing is performed through two main paths:

- Windows application
- Web application, through the «مدیریت عملیات چاپ» form

### 2.2. Reprinting

Reprinting takes place when a serial has already been created or registered and the user wants to print its label or specification sheet again.

Reprinting is provided in both the Windows and Web versions.

In the Windows version, the reprint option receives the serial number, after which the print template is selected and the printing operation is performed.

In the Web version, the «مدیریت عملیات چاپ» form allows users to search by serial number. The system identifies the operation code associated with that serial and then makes the relevant templates available to the user.

The reprint template does not necessarily have to be the same as the original print template. Other templates may also be selected, provided they are available for the relevant form and scenario.

### 2.3. Print Status and Number of Prints

In some workflows, the print status and the number of print attempts can also be tracked. However, a distinction must be made between **serial issuance**, **print operation preparation**, and **successful physical printing**. Preparing a print operation or recording a print request does not, by itself, prove that the label was actually printed.

---

## 3. Print Templates in the Windows Version

### 3.1. Concept of a Print Template

A print template defines how data and printable elements are arranged on a label or sheet and how they are converted into a printable output.

Three main template structures were discussed for the Windows application:

- FastReport
- ZPL
- ZMin

These templates differ in their design methods, command delivery mechanisms, and output types.

### 3.2. FastReport

FastReport is a tool for visually designing print templates.

With this approach, the designer can place report or label elements in the design environment and define their layout.

According to the session explanations, FastReport templates in the Windows application can be used for two output paths:

- Sending output directly to a printer
- Generating a PDF file

In the PDF workflow, FastReport generates the document structure, and the resulting file is made available to the user for viewing or printing.

### 3.3. ZPL and Zebra Printers

**ZPL is a print command language used by Zebra printers and devices compatible with this language.**

ZPL stands for Zebra Programming Language. With this approach, the print template is defined as text-based commands. Unlike the visual design approach used by FastReport, the position and properties of printable elements are specified through print-language commands.

Elements that can be defined in a ZPL template include:

- Element positions on the label
- Element dimensions and layout
- Printable text
- Barcodes
- Images and other elements supported by the printer

In the Windows application, ZPL template files are stored in a folder named `ZPLFormats`. These files usually contain text-based printing commands.

To use a ZPL template, the printer must support ZPL or the relevant commands. Printer and template compatibility must be verified; not every printer can necessarily execute every ZPL template.

**Important:** ZPL is the name of a print command language/template format, not a generic name for all printers. Zebra is a manufacturer and a family of printers. Compatibility with ZPL must be verified based on the device model and its supported capabilities.

### 3.4. ZMin

ZMin is another print template structure discussed for the Windows application.

Like ZPL, this template is stored as text-based commands and is used with printers that support the relevant protocol or command structure.

Files for this template type are stored in the `ZMinFormats` folder.

During the session, ZMin was described as similar to ZPL in that its templates are text-based and define printable elements through commands. The exact command syntax and compatibility with different printer models must be verified against the actual ZMin implementation.

It must not be assumed that every printer compatible with ZPL also supports ZMin.

### 3.5. Differences Between FastReport, ZPL, and ZMin

**FastReport:** A template designed using a visual design tool. In supported workflows, it can produce direct printer output or a PDF file.

**ZPL:** A text-based template containing print commands for printers that support ZPL, including compatible Zebra printers.

**ZMin:** A text-based template based on print commands for devices or implementations that support ZMin.

Template selection depends on the printer type, its capabilities, the required label structure, and the output method.

### 3.6. Difference Between a Print Template File and a Report Template

A print template and a report template are different concepts and must not be confused.

A print template may be a FastReport file, a Stimulsoft file, or a collection of text-based commands such as ZPL or ZMin.

These templates are defined as files or storable content. A report, by contrast, may have a different purpose and structure.

When documenting or troubleshooting an issue, it must be clarified which type of template is meant.

---

## 4. Storage Locations for Print Template Files

### 4.1. FastReport Files

In the Windows application, FastReport files are stored in the `PrintFormats` folder at the application's root directory.

### 4.2. ZPL Files

Text files containing ZPL commands are stored in the `ZPLFormats` folder.

### 4.3. ZMin Files

Text files containing ZMin commands are stored in the `ZMinFormats` folder.

### 4.4. Stimulsoft Files in the Web Version

In the Web version, print templates are designed using Stimulsoft and registered or uploaded through the «مدیریت قالب‌های چاپ» form in the system.

According to the session explanations, the system stores template files in a folder named `Reports`.

The physical storage location must be verified for each environment based on its installation structure and configuration.

---

## 5. Printer Selection in the Windows Version

### 5.1. Default Printer

A default printer can be specified in the Windows application's settings.

If no dedicated printer has been assigned to the selected template, the system can use the default printer.

### 5.2. Dedicated Printer for Each Template

A dedicated printer can also be assigned to each print template.

For example, if several printers are available, different templates can be associated with different printers so that each template is sent to an appropriate device.

### 5.3. Printer Selection Conditions

Printer selection must be compatible with the template type and the device's capabilities.

For example, a ZPL template must be sent to a printer that supports the relevant commands. Selecting an incompatible printer may result in an incorrectly printed label or misinterpreted commands.

---

## 6. Multiple Templates for One Serial

Silo supports scenarios in which more than one print template is required for a single serial number.

For example, a single serial may require:

- A label containing identification information
- A specification sheet in A4 format
- Another sheet in A5 format with a different template or different information

Therefore, one serial number is not necessarily limited to a single print template.

In the Windows application, available templates may include FastReport, ZPL, and ZMin. The operator can select the required template, and printing is performed through the default printer or the printer assigned to that template.

The availability of each template depends on the system settings and its association with the relevant workflow.

---

## 7. Relationship Between Print Templates and Serial Information

### 7.1. Filtering Templates Based on Product or Serial Information

The system can be configured to display templates relevant to the serial or product information.

For example, if a template is defined for a specific brand or category, the associated information can be used to restrict the list of templates displayed.

As a result, instead of displaying every available template, the system can suggest only templates relevant to the data associated with that serial, provided that the corresponding template association and filtering rules have been implemented.

### 7.2. Association Through the Template Name

During the session, it was explained that this association can be implemented through keys, identifiers, or symbols defined in the template name.

With this approach, the internal content of the template file does not necessarily need to be changed. Association information may be included in the file name or template title, allowing the system to identify relevant templates based on that information.

The exact naming conventions and identification rules must be determined from the actual system configuration and implementation. A specific naming pattern must not be presented as a definitive system standard without verification.

---

## 8. Print Management in the Web Version

### 8.1. «مدیریت عملیات چاپ» Form

In the Web version, the «مدیریت عملیات چاپ» form is available for selecting a serial and executing the printing workflow.

The system key for this form is `Print Management`. The title displayed to users may be configurable; therefore, the displayed title and the system key must not automatically be assumed to be identical.

### 8.2. Stimulsoft Templates

In the Web version, print templates are designed using Stimulsoft Reports.

Designing a template file alone is not sufficient to make it available for use. The template must be registered in the system through the «مدیریت قالب‌های چاپ» form and associated with the relevant form, such as `Print Management`.

During this process, information such as the template file and its target form is specified.

Once the template is associated with the form, the system can display templates linked to that form when the printing operation is executed.

#### 8.2.1. Accessing the Print Template Management Form

In the Web version, print templates are registered and managed through a dedicated form used for storing and accessing print templates.

The displayed name of this form may differ between projects. Therefore, the form must not be identified by one fixed title across all Silo projects.

#### 8.2.2. Registering a Print Template

To register a new print template, the user must complete the following information:

- **Template Title:** A title must be assigned to the print template. This title is used to identify the template in the system.
- **File Address:** The address of the template file must be entered in the File Address field. The template file is stored in the `Reports` folder.
- **Operation Type:** An operation type can be specified for each print template.
- **Target Forms / Pages:** The user must select the pages or forms in which the print template should be displayed. This determines where the template is available for selection.

After completing the required information, the user clicks Save to register the print template.

#### 8.2.3. Form Controls

The print template management form provides the following controls:

- **Refresh:** Refreshes the information entered in the form.
- **Save:** Saves the entered print template information.
- **Template List:** A gray button with a list icon allows the user to view previously registered print templates.

#### 8.2.4. Viewing and Editing Registered Templates

By clicking the gray Template List button with the list icon, users can view the print templates that have already been registered in the system.

Users can select an existing template and edit its information when necessary.

The available templates and their visibility depend on the forms or pages associated with each template.

Users can also delete an existing print template when necessary.

### 8.3. Printing Workflow in the Web Version

The general workflow is as follows:

1. The required serial is selected or searched for.
2. Templates registered and associated with the «مدیریت عملیات چاپ» form are displayed.
3. The user selects the required template.
4. A PDF output is generated.
5. The user views and prints the PDF using standard document viewing and printing tools.

### 8.4. Limitations on Direct Printer Connectivity in the Web Version

In the state described during the session, the Web version does not communicate directly with the printer to perform physical printing. Instead, the print output is provided as a PDF.

Therefore, direct transmission of ZPL or ZMin commands to a printer must not be assumed to be supported by the Web form. Direct printing and these template formats were described for the Windows version, whereas the Web workflow uses Stimulsoft templates and PDF generation.

---

## 9. Bluetooth Jet Printers and Printing from Android

A jet printer is a device used for direct printing on various surfaces, such as tiles, stone, or glass. Some models support Bluetooth connectivity.

During the session, the need to send print commands for a serial generated on Android directly to a Bluetooth jet printer was discussed.

This capability was described as a technical gap and a planned future enhancement, rather than a generally operational capability in the current state.

Therefore:

- Generating or receiving a serial on Android must not be treated as equivalent to supporting label printing on Android.
- Direct printing to a Bluetooth jet printer is not a completed, generally available capability in the state described.
- For direct printing from Android, the development status and compatibility with the specific device must be verified separately.

---

## 10. Differences Between the Windows, Web, and Android Versions in Printing

### Windows Version

- Support for FastReport-, ZPL-, and ZMin-based printing workflows in relevant scenarios
- PDF generation through FastReport
- Ability to select a default printer
- Ability to assign a dedicated printer to each template
- Ability to use multiple templates for one serial
- Reprinting capability
- Storage of templates in designated application folders

### Web Version

- Use of Stimulsoft Reports to design print templates
- Registration of templates and association with the «مدیریت عملیات چاپ» form
- PDF output generation for printing
- Reprinting through the «مدیریت عملیات چاپ» form
- No direct printer connectivity in the state described during the session

### Android Version

- Support for online and offline serial issuance scenarios according to configuration
- Ability to record requests for later transmission to the server in offline mode
- Ability to generate serials locally in scenarios using SQLite formulas
- No generally available, completed label-printing support in the state described
- Direct printing to Bluetooth jet printers identified as a planned future enhancement

This comparison reflects the capabilities described during the session. Any subsequent software changes must be verified against the actual version in use.

---

## 11. Guidelines for the Agent When Answering User Questions

### 11.1. Questions About Zebra Printers

If the user asks about a Zebra printer or a ZPL template, explain that ZPL is a print command language used by Zebra printers and compatible devices. In the Windows version, ZPL template files are stored in the `ZPLFormats` folder.

If a specific printer model is mentioned, do not definitively confirm ZPL support without reliable information about that model and its compatibility.

### 11.2. Questions About the Difference Between ZPL and FastReport

Explain that FastReport is a visual template design tool that supports direct printing or PDF generation in supported workflows. ZPL, by contrast, is a text-based template format built around print commands for compatible printers.

These approaches differ in template design, printer compatibility, and output generation.

### 11.3. Questions About Print Template File Locations

- FastReport in the Windows version: `PrintFormats`
- ZPL in the Windows version: `ZPLFormats`
- ZMin in the Windows version: `ZMinFormats`
- Stimulsoft templates in the Web version: `Reports`, according to the session explanations

If the question concerns the exact installation path or an actual file in a specific environment, inspect that environment's configuration rather than guessing the full path.

### 11.4. Questions About Printing from the Web Version

Explain that, in the state described, the Web version uses Stimulsoft templates and generates PDF output. Physical printing is performed using standard PDF viewing and printing tools.

Do not confirm direct command transmission to the printer unless there is evidence of a subsequent implementation change.

### 11.5. Questions About Printing from Android

Generating a serial on Android does not mean that direct printing is supported. In the state described, printing is available through the Windows and Web versions, while direct transmission of print commands to a Bluetooth jet printer on Android was identified as a planned future enhancement.

### 11.10. Questions About Reprinting

Explain that reprinting is used for a serial that has already been created or registered. Reprinting workflows have been described for both the Windows and Web versions, and the user can select an available template.

---

## 12. Inference Limitations and Rules for Preventing Incorrect Answers

1. Preparing a print operation does not, by itself, mean that physical printing has succeeded.

2. Generating a serial on Android does not, by itself, mean that direct printing is supported on Android.

3. The existence of a ZPL template does not mean that all printer models are compatible with ZPL.

4. A printer's support for ZPL does not imply support for ZMin.

5. The paths and folder names listed in this document are those mentioned during the session. To provide the complete path on a specific server, the installation environment must be inspected.

6. Capabilities that are planned or have known technical gaps, such as direct printing from Android to a Bluetooth jet printer, must not be presented as definitively operational.

7. Implementation details not specified in this document, such as the exact file-naming convention used for intelligent template filtering, must not be filled in by guessing.

8. Serial issuance, creation of a print record, physical printing, RFID reading, tag registration, and warehouse entry are separate events. Completion must be confirmed only on the basis of a reliable result for the relevant operation.

9. If behavior depends on the project, version, permissions, or configuration, and reliable information is unavailable, explicitly state this limitation. Do not fill in the missing details by guessing.

---

## 13. Summary of Capabilities

For printing, the Windows version supports FastReport, ZPL, and ZMin templates. ZPL is a print command language for Zebra printers and compatible devices. Template files are stored in the `PrintFormats`, `ZPLFormats`, and `ZMinFormats` folders. Other capabilities discussed include selecting a default printer or assigning a dedicated printer to a template, using multiple templates for one serial, and reprinting.

The Web version uses Stimulsoft templates and, in the state described, provides PDF output. Direct printer connectivity is not available in this version as described. Label printing through Android is also not considered a generally operational capability in the state covered by the session; direct printing to a Bluetooth jet printer was identified as a planned future enhancement.

When answering users, the agent must always distinguish between existing capabilities, configuration-dependent capabilities, technical limitations, and planned future enhancements. Details that have not been confirmed by the available knowledge must not be presented as established facts.