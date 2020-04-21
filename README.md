# Handling Solr Managed Schema through Sitecore config files
By default, Sitecore uses a built-in, hard coded, SchemaPopulateHelper
class that handles updates of the Solr Managed schema. When changes
are needed to the schema, a lot of code needs to be overridden and things
becomes pretty complex to handle.

This module replaces the built in SchemaPopulateHelper and allows
writing the Solr schema as plain XML within the Sitecore config system.
This allows using the Sitecore config patch system. It also allows cleaner
configuration, as the Sitecore index configuration field mapping, 
document options etc can be placed together with corresponding schema
changes where applicable.

The module is written for Sitecore 9.3, but can easily be adjusted to work 
with previous versions. Note that the ISchemaPopulateHelper inteface is
slightly different in earlier versions, but the concept is the same since
Sitecore 9.0.

