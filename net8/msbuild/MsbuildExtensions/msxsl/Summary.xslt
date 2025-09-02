<?xml version="1.0" encoding="UTF-8" ?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:vs="http://microsoft.com/schemas/VisualStudio/TeamTest/2010">
<xsl:output method='text' encoding="ascii"/>

<!--
TestOutcome
10:Passed
 7:NotExecuted
-->

<xsl:template match="text()"/>

<xsl:template match="/">
    <xsl:variable name="total" select="/vs:TestRun/vs:ResultSummary/vs:Counters/@total"/>
    <xsl:variable name="executed" select="/vs:TestRun/vs:ResultSummary/vs:Counters/@executed"/>
    <xsl:variable name="failed" select="/vs:TestRun/vs:ResultSummary/vs:Counters/@failed"/>

    <xsl:text>TestsRun:</xsl:text>
    <xsl:value-of select="$executed"/>
    <xsl:text> Failures:</xsl:text>
    <xsl:value-of select="$failed"/>
    <xsl:text> NotRun:</xsl:text>
    <xsl:value-of select="$total - $executed"/>

</xsl:template>

</xsl:stylesheet>
