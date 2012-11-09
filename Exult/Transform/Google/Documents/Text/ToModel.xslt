<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet xmlns:msxsl="urn:schemas-microsoft-com:xslt" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" exclude-result-prefixes="msxsl" version="1.0" xml:space="default" >
  <xsl:output indent="yes" method="xml" omit-xml-declaration="yes"/>
  
  <xsl:param name="metadata" select="/.." />

  <xsl:variable name="lower" select="'abcdefghijklmnopqrstuvwxyz'" />
  <xsl:variable name="upper" select="'ABCDEFGHIJKLMNOPQRSTUVWXYZ'" />

  <!--=============
      Root Template
      =============-->
  <xsl:template match="/">
    <xm:document xmlns:xm='http://code.google.com/p/exult/model' xmlns:xb='http://code.google.com/p/exult/binding'>
      <xsl:apply-templates select="$metadata//@*" mode="cp"/>
      <xsl:apply-templates select="/*/*/*[translate(local-name(), $upper, $lower)='style']" mode="style"/>
      <xsl:apply-templates select="/*/*[translate(local-name(), $upper, $lower)='body']/node()"  mode="body"/>
    </xm:document>
  </xsl:template>

  <xsl:template match="*" priority="2" mode="body">
    <xsl:element name="{local-name()}">
      <xsl:attribute name="xb:class" xmlns:xb="http://code.google.com/p/exult/binding">
        <xsl:text>body</xsl:text>
      </xsl:attribute>
      <xsl:apply-templates mode="cp" select="@* | node()" />
    </xsl:element>
  </xsl:template>
  
  <xsl:template match="text()" priority="2" mode="body">
    <div xb:class="body" xmlns:xb="http://code.google.com/p/exult/binding">
      <xsl:apply-templates mode="cp" select="@* | node() | text()" />
    </div>
  </xsl:template>

  <xsl:template match="@*" mode="body"/>
  <xsl:template match="node()" mode="body"/>

  <xsl:template match="*" priority="2" mode="style">
    <xsl:element name="{local-name()}">
      <xsl:attribute name="xb:class" xmlns:xb="http://code.google.com/p/exult/binding">
        <xsl:text>style</xsl:text>
      </xsl:attribute>
      <xsl:apply-templates mode="cp" select="@* | node()" />
    </xsl:element>
  </xsl:template>

  <xsl:template match="text()" priority="2" mode="style">
    <div xb:class="style" xmlns:xb="http://code.google.com/p/exult/binding">
      <xsl:apply-templates mode="cp" select="@* | node() | text()" />
    </div>
  </xsl:template>

  <xsl:template match="@*" mode="style"/>
  <xsl:template match="node()" mode="style"/>
  
  <!--==============
      View Model Copy
      ==============-->
  <xsl:template match="@* | node() | text()" mode="cp">
    <xsl:copy>
      <xsl:apply-templates mode="cp" select="@* | node() | text()" />
    </xsl:copy>
  </xsl:template>
</xsl:stylesheet>