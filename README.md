CALC Emulator.
-
The CALC Emulator is a Console App that can assemble a custom made assembly code to a custom machine code and can also execute the machine code. <br>
The output machine code of this assembler/emulator is used in my minecraft hexadecimal redstone computer (CALC). <br>
This assembler uses ".asc" files and converts them to ".smc" files. <br>

Writing a ASC code.
-
<ul class="a">
  <li>Create a new .asc file.</li>
  <li>Write "Main:" to start of with.</li>
  <li>alternatively you can use the "base.asc" file.</li>
  <li>To make a method use "def" followed by method name.</li>
  <li>Then add a "RTN" instruction at the end of the method.</li>
  <li>To call this method in the "Main:" block use "CAL" followed by method pointer.</li>
</ul>

Example ASC code.
-
Example code can be found in the "<a href="CALC-Emulator/example">example</a>" folder.

Downloads/Links.
-
CALC ISA: <a href="https://docs.google.com/spreadsheets/d/1cAkJrPHr2NaB6NzkKBMjP4aPQQalOxp-QOafpjYkXTk/edit?usp=sharing" target=”_blank”>CALC ISA</a><br>
Base ASC File: <a href="base.asc" download="base.asc">base.asc</a><br>
