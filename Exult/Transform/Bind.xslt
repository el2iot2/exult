<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet xmlns:msxsl="urn:schemas-microsoft-com:xslt" xmlns:xb="http://code.google.com/p/exult/binding" xmlns:xm="http://code.google.com/p/exult/model" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:xvm="http://code.google.com/p/exult/viewmodel" exclude-result-prefixes="msxsl xm xb xvm" version="1.0" xml:space="default">
  <xsl:output indent="yes" method="xml" omit-xml-declaration="yes" />
  <xsl:strip-space elements="*" />
  <xsl:param name="ViewModel" select="/.." />
  <xsl:param name="Model" select="/.." />

  <!--=============
      Root Template
      =============-->
  <xsl:template match="/">
    <xsl:apply-templates mode="vm" select="$ViewModel" />
  </xsl:template>

  <!--================
      Document Wrapper
      ================-->
  <xsl:template match="/xvm:document" mode="vm">
    <!--Inject a preamble (doctype/xml declaration/etc.) if there is one-->
    <xsl:if test="xvm:preamble">
      <xsl:value-of disable-output-escaping="yes" select="xvm:preamble/text()"  />
    </xsl:if>
    <!--Then go on to the view model-->
    <xsl:apply-templates mode="vm" />
  </xsl:template>

  <!--=================
      Suppress Preamble
      =================-->
  <xsl:template match="xvm:preamble" mode="vm">
    <!--Preamble has been already injected. Ignore it.-->
  </xsl:template>

  <xsl:template name="get-value">
    <xsl:param name="name" />
    <xsl:param name="type" />
    <xsl:param name="namespace" select="''" />
    <xsl:choose>
      <xsl:when test="$type='.'">
        <xsl:apply-templates mode="model_class" select="msxsl:node-set($Model)/*">
          <xsl:with-param name="name" select="$name" />
          <xsl:with-param name="namespace" select="$namespace" />
        </xsl:apply-templates>
      </xsl:when>
      <xsl:when test="$type='#'">
        <xsl:apply-templates mode="model_element" select="msxsl:node-set($Model)/*">
          <xsl:with-param name="name" select="$name" />
          <xsl:with-param name="namespace" select="$namespace" />
        </xsl:apply-templates>
      </xsl:when>
      <xsl:when test="$type='@'">
        <xsl:apply-templates mode="model_attribute" select="msxsl:node-set($Model)/*/@*">
          <xsl:with-param name="name" select="$name" />
        </xsl:apply-templates>
      </xsl:when>
    </xsl:choose>
  </xsl:template>

  <!--=============================
      Simple Bound Attribute (AttrBinding) yada="=>@Something"
      =============================-->
  <xsl:template match="@*[starts-with(.,'=&gt;@')]" mode="vm">
    <xsl:variable name="value_source" select="substring(.,3)" />
    <xsl:variable name="value_source_name" select="substring($value_source,2)" />
    <xsl:variable name="value_source_type" select="substring($value_source,1,1)" />
    <xsl:variable name="value">
      <xsl:call-template name="get-value">
        <xsl:with-param name="name" select="$value_source_name" />
        <xsl:with-param name="type" select="$value_source_type" />
      </xsl:call-template>
    </xsl:variable>
    <xsl:if test="$value">
      <xsl:attribute name="{name()}" namespace="{namespace-uri()}"><xsl:value-of select="$value" /></xsl:attribute>
    </xsl:if>
  </xsl:template>

  <!--=============================
      Substitution Bound Attribute (AttrBinding) yada="=>Prefix/@{Something}/Suffix"
      =============================-->
  <xsl:template match="@*[starts-with(.,'=&gt;') and contains(.,'@{')]" mode="vm">
    
    <!-- "=>Prefix/@{Something}/Suffix" -> "Prefix/@{Something}/Suffix" -->
    <xsl:variable name="value_expression" select="substring-after(.,'=&gt;')" />

    <!-- "Prefix/@{Something}/Suffix" -> "Prefix/" -->
    <xsl:variable name="prefix" select="substring-before($value_expression,'@{')" />

    <!-- "Prefix/@{Something}/Suffix" -> "Something}/Suffix" -->
    <xsl:variable name="value_expression_remainder" select="substring-after($value_expression,'@{')" />

    <!-- "Something}/Suffix" -> "Something" -->
    <xsl:variable name="value_source_name" select="substring-before($value_expression_remainder,'}')" />

    <!-- "Something}/Suffix" -> "/Suffix" -->
    <xsl:variable name="suffix" select="substring-after($value_expression_remainder,'}')" />

    <xsl:variable name="value">
      <xsl:call-template name="get-value">
        <xsl:with-param name="name" select="$value_source_name" />
        <xsl:with-param name="type" select="'@'" />
      </xsl:call-template>
    </xsl:variable>
    
    <xsl:if test="$value">
      <xsl:attribute name="{name()}" namespace="{namespace-uri()}">
        <xsl:value-of select="concat($prefix,$value,$suffix)" />
      </xsl:attribute>
    </xsl:if>
  </xsl:template>

  <!--======================
      ID Binding (IdBinding)
      ======================-->
  <xsl:template match="*[contains(@id,'=&gt;')]" mode="vm">
    <!--Precompute our variables of interest-->
    <xsl:variable name="target_xpath" select="substring-before(@id,'=&gt;')" />
    <xsl:variable name="rhs" select="substring-after(@id,'=&gt;')" />
    <xsl:variable name="value_source">
      <xsl:choose>
        <xsl:when test="contains($rhs,':')">
          <xsl:value-of select="substring-before($rhs,':')" />
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="$rhs" />
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:variable name="value_source_name" select="substring($value_source,2)" />
    <xsl:variable name="residual_id">
      <xsl:choose>
        <xsl:when test="contains($rhs,':')">
          <xsl:value-of select="substring-after($rhs,':')" />
        </xsl:when>
        <xsl:otherwise>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:variable name="value_source_type" select="substring($value_source,1,1)" />
    <xsl:variable name="value">
      <xsl:call-template name="get-value">
        <xsl:with-param name="name" select="$value_source_name" />
        <xsl:with-param name="type" select="$value_source_type" />
        <xsl:with-param name="namespace" select="namespace-uri()" />
      </xsl:call-template>
    </xsl:variable>


    <!--Switch on the target xpath-->
    <xsl:choose>

      <!--REPLACE THIS ELEMENT-->
      <xsl:when test="$target_xpath='.' or not($target_xpath)">
        <xsl:apply-templates mode="inject" select="msxsl:node-set($value)" />
      </xsl:when>

      <!--REPLACE THIS ELEMENT'S TEXT-->
      <xsl:when test="$target_xpath='text()'">
        <!--Build an element-->
        <xsl:element name="{name()}" namespace="{namespace-uri()}">
          <!--With all existing attributes (except 'id')-->
          <xsl:apply-templates mode="vm" select="@*[name() != 'id']" />
          <!--And then the residual id-->
          <xsl:if test="string-length($residual_id) &gt; 0">
            <xsl:attribute name="id">
              <xsl:value-of select="$residual_id" />
            </xsl:attribute>
          </xsl:if>
          <!--And then the new text-->
          <xsl:value-of select="string($value)" />
        </xsl:element>
      </xsl:when>

      <!--REPLACE THIS ELEMENT'S CHILDREN-->
      <xsl:when test="$target_xpath='*'" xml:space="default" >
        <!--Build an element-->
        <xsl:element name="{name()}" namespace="{namespace-uri()}">
          <!--With all existing attributes (except 'id')-->
          <xsl:apply-templates mode="vm" select="@*[name() != 'id']" />
          <!--And then the residual id-->
          <xsl:if test="$residual_id and $residual_id != ''">
            <xsl:attribute name="id">
              <xsl:value-of select="$residual_id" />
            </xsl:attribute>
          </xsl:if>
          <xsl:apply-templates mode="inject" select="msxsl:node-set($value)" />
        </xsl:element>
      </xsl:when>
    </xsl:choose>
  </xsl:template>

  <!--==============
      View Model Copy
      ==============-->
  <xsl:template match="@* | node()" mode="vm">
    <xsl:copy>
      <xsl:apply-templates mode="vm" select="@* | node()" />
    </xsl:copy>
  </xsl:template>

  <!--==============
      Inject : Exclude Filter Binding and model/binding namespaces
      ==============-->
  <xsl:template match="@*[namespace-uri() = 'http://code.google.com/p/exult/binding'] | node()[namespace-uri() = 'http://code.google.com/p/exult/binding']" mode="inject" />

  <!--==============
      Inject : Migrate Model Element
      ==============-->
  <xsl:template match="*" mode="inject">
    <xsl:param name="namespace" />
    <xsl:element name="{local-name()}" namespace="{$namespace}">
      <xsl:apply-templates mode="inject" select="node()|@*">
        <xsl:with-param name="namespace" select="$namespace" />
      </xsl:apply-templates>
    </xsl:element>
  </xsl:template>

  <!--==============
      Inject : Handle HTML Entity
      ==============-->
  <xsl:template match="ent" mode="inject">
    <xsl:text disable-output-escaping="yes">&amp;#</xsl:text>
    <xsl:value-of select="@code"/>
    <xsl:text>;</xsl:text>
  </xsl:template>

  <!--==============
      Inject : Migrate Model attribute
      ==============-->
  <xsl:template match="@*" mode="inject">
    <xsl:param name="namespace" />
    <xsl:attribute name="{local-name()}" namespace="{$namespace}">
      <xsl:value-of select="string(.)" />
    </xsl:attribute>
  </xsl:template>

  <!--==============
      Inject : Catchall Copy
      ==============-->
  <xsl:template match="text()|comment()|processing-instruction()" mode="inject">
    <xsl:param name="namespace" />
    <xsl:copy>
      <xsl:apply-templates mode="inject" select="@* | node()">
        <xsl:with-param name="namespace" select="$namespace" />
      </xsl:apply-templates>
    </xsl:copy>
  </xsl:template>

  <!--==============
      Model Class Copy
      ==============-->
  <xsl:template match="/*/*[@xb:class]" mode="model_class">
    <xsl:param name="name" />
    <xsl:param name="namespace" />
    <xsl:if test="@xb:class=$name">
      <xsl:apply-templates mode="inject" select=".">
        <xsl:with-param name="namespace" select="$namespace" />
      </xsl:apply-templates>
    </xsl:if>
  </xsl:template>

  <!--==============
      Model Class Traverse
      ==============-->
  <xsl:template match="@* | node()" mode="model_class">
    <xsl:param name="name" />
    <xsl:param name="namespace" />
    <xsl:apply-templates mode="model_class" select="@* | node()">
      <xsl:with-param name="name" select="$name" />
      <xsl:with-param name="namespace" select="$namespace" />
    </xsl:apply-templates>
  </xsl:template>

  <!--==============
      Model Attribute Copy
      ==============-->
  <xsl:template match="/*/@*" mode="model_attribute">
    <xsl:param name="name" />
    <xsl:if test="name()=$name">
      <xsl:value-of select="string(.)" />
    </xsl:if>
  </xsl:template>

  <!--==============
      Model Attribute Traverse
      ==============-->
  <xsl:template match="@* | node()" mode="model_attribute">
    <xsl:param name="name" />
    <xsl:apply-templates mode="model_attribute" select="@* | node()">
      <xsl:with-param name="name" select="$name" />
    </xsl:apply-templates>
  </xsl:template>

  <!--==============
      Model Element Copy
      ==============-->
  <xsl:template match="/*/*[@xb:id]" mode="model_element">
    <xsl:param name="name" />
    <xsl:param name="namespace" />
    <xsl:if test="@xb:id=$name">
      <xsl:apply-templates mode="inject" select=".">
        <xsl:with-param name="namespace" select="$namespace" />
      </xsl:apply-templates>
    </xsl:if>
  </xsl:template>
  <!--==============
      Model Element Traverse
      ==============-->
  <xsl:template match="@* | node()" mode="model_element">
    <xsl:param name="name" />
    <xsl:param name="namespace" />
    <xsl:apply-templates mode="model_element" select="@* | node()">
      <xsl:with-param name="name" select="$name" />
      <xsl:with-param name="namespace" select="$namespace" />
    </xsl:apply-templates>
  </xsl:template>

</xsl:stylesheet>