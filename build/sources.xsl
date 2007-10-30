<?xml version='1.0'?>
<xsl:stylesheet version="1.0"
								xmlns:x="http://schemas.microsoft.com/developer/msbuild/2003"
								xmlns:xsl="http://www.w3.org/1999/XSL/Transform">

	<xsl:output method="text" encoding="ascii" indent="yes"/>

	<xsl:template match="/">
		<xsl:apply-templates select="x:Project/x:ItemGroup/x:Compile"/>
	</xsl:template>

	<xsl:template match="x:Compile[not(@Condition)]">
		<xsl:value-of select="@Include" /><xsl:text> </xsl:text>
	</xsl:template>

</xsl:stylesheet>
