## Inventory Aging Reporting Service
This Windows service application is designed to generate and maintain inventory aging reports for companies that don't use batch numbering. It ignores the warehouse data and transfer transactions, offering a company-wide age of the material. The solution periodically retrieves inventory data from an source system's database (in this case an ERP), calculates aging metrics, and updates a data warehouse with the results.

## Features

• Automated inventory aging calculation

• Periodic execution based on configurable job schedules

• Support for multiple aging periods (0-30 days, 31-60 days, etc., up to 1801+ days)

• Separate tracking for quantities and weights

• Efficient database operations for inserting, updating, and deleting report entries
