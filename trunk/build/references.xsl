<?xml version='1.0'?>
<xsl:stylesheet version="1.0"
								xmlns:x="http://schemas.microsoft.com/developer/msbuild/2003"
								xmlns:xsl="http://www.w3.org/1999/XSL/Transform">

	<xsl:output method="text" encoding="ascii" indent="yes"/>

	<xsl:template match="/">
		<xsl:apply-templates select="x:Project/x:ItemGroup/x:Reference"/>
		<xsl:apply-templates select="x:Project/x:ItemGroup/x:ProjectReference"/>
	</xsl:template>

	<xsl:template match="x:Reference">-r:<xsl:value-of select="@Include" />.dll<xsl:text> </xsl:text></xsl:template>
	<xsl:template match="x:ProjectReference">-r:<xsl:value-of select="x:Name" />.dll<xsl:text> </xsl:text></xsl:template>

</xsl:stylesheet>
