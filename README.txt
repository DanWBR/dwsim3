==========================================
DWSIM - Open Source Process Simulator
Copyright (c) Daniel Wagner O. de Medeiros
==========================================

DWSIM is a software for modeling, simulation and optimization of steady-state chemical processes.

==========================================
DISCLAIMER
==========================================

The data and information within DWSIM has been obtained from a wide variety of literature sources.  While reasonable care has been exercised in the collection of data and testing of this software, the author of DWSIM disclaims any warranty, expressed or implied, as to the accuracy or reliability of the data or calculations contained therein. The results of calculations obtained from DWSIM yield approximate results, which will not always be suitable for every application.

The software is designed for use by trained professional personnel and is not a substitute for sound professional judgment.  It is the sole responsibility of the user to validate the data presented by DWSIM and to determine whether the results of this program are accurate and suitable for any specific purpose.  No guarantee of accuracy or fitness for any purpose is expressed or implied.  The author strongly recommends that the data be checked against other sources and/or methods before use and application.  The author shall not be held liable for any direct, indirect, consequential or incidental damages incurred through use of the data or calculations. 

==========================================
LICENSE
==========================================

DWSIM is licensed under the GNU General Public License (GPL) Version 3.
 
==========================================
SOFTWARE/SYSTEM REQUIREMENTS
==========================================

OS: 	Windows XP/2000/Vista/7/8
Software: 	.NET Framework 4.0 / Mono 2.10 or newer
CPU: 	1.6 GHz single-core processor (minimum)
Memory: 	512 MB RAM
HD space: 	200 MB for program files.
Display: 	A 1024x768 display resolution is recommended as a minimum.

==========================================
COMPILATION TIPS
==========================================

DWSIM is a VB.NET project that compiles for the .NET 4.0 profile. 
Compile DWSIM on Windows using Visual Studio 2010, Visual Basic 2010 Express Edition or SharpDevelop. MonoDevelop is not supported.

==========================================
USAGE INFO (LINUX / OS X)
==========================================

** Linux

To run DWSIM on Linux, open a terminal (console) window, point it to the folder which contains the DWSIM executable and execute the following command:

mono DWSIM.exe

To run in debug mode, include the '--debug' switch (when you encounter an unhandled exception, debug mode will include information about the source code file and line number where the exception was raised - helps to track bugs):

mono --debug DWSIM.exe

In order to use the PC-SAFT Property Package, the Gibbs Reactor and the IPOPT solver, you'll need to copy the corresponding native libraries to your 'lib' folder (you'll need to do this only once) and rename 2 files in the DWSIM directory:

sudo cp sharedobjects\liblpsolve55.so /usr/lib/
sudo cp sharedobjects\libPC_SAFT_PROP.so /usr/lib/
sudo cp sharedobjects\libfprops_ascend.so /usr/lib/
sudo tar -C /usr/lib -zxvf sharedobjects\libipopt_mono_dwsim_ubuntu_11.10_32.tar.gz

mv DWSIM.exe.config.linux DWSIM.exe.config
mv Cureos.Numerics.dll.config.linux Cureos.Numerics.dll.config

On Linux machines, Mono can locate these libraries on the DWSIM folder, so you don't need to copy them.

** OS X

Important: Mono requires the X11 package installed on OS X in order to run Windows Forms applications, including DWSIM. Be sure to have it installed before doing the steps below.

To run DWSIM on OS X, open a terminal (console) window (Go > Utilities > Terminal), point it to the folder which contains the DWSIM executable and execute the following commands:

export MONO_MWF_MAC_FORCE_X11=1
mono DWSIM.exe

To run in debug mode, include the '--debug' switch (when you encounter an unhandled exception, debug mode will include information about the source code file and line number where the exception was raised - helps to track bugs):

mono --debug DWSIM.exe

In order to use the PC-SAFT Property Package, the Gibbs Reactor and the IPOPT solver, you'll need to copy the corresponding native libraries to your 'lib' folder (you'll need to do this only once) and rename 2 files in the DWSIM directory:

sudo cp dynamiclibraries\liblpsolve55.dylib /usr/lib/
sudo cp dynamiclibraries\libgfortran.3.dylib /usr/lib/
sudo cp dynamiclibraries\libPC_SAFT_PROP.dylib /usr/lib/
sudo cp dynamiclibraries\libfprops_ascend.dylib /usr/lib/
sudo unzip dynamiclibraries\libipopt_mono_dwsim_osx_10.7.3_32.zip -d /usr/lib

mv DWSIM.exe.config.osx DWSIM.exe.config
mv Cureos.Numerics.dll.config.osx Cureos.Numerics.dll.config

===============================================
KNOWN ISSUES
===============================================

Known limitations of DWSIM when running on Mono:

- Report Tool doesn't work. When the user clicks on the button to generate a report preview, a blank page appears;
- DataGrids don't display tooltips, this is a Mono limitation, not DWSIM's.

==========================================
VERSION HISTORY / CHANGELOG
==========================================

Version 3.0 Build 4700

- [NEW] Unified code base and single executable for .NET/Mono, compiled for CLR v4.0
- [NEW] New XML simulation file format for full compatibility between platforms (Windows/Linux/OS X)
- [NEW] New Parallel calculations option

Version 2.1 Build 4680

- [NEW] Save selected object properties to text file
- [FIX] Fixed PFR, CSTR and Pump models
- [FIX] Fixed instability on the Nested Loop PH Flash code
- [FIX] Fixed pressure unit conversion from barg to Pa

Version 2.1 Build 4606

- [CHG] COSMO-SAC database loading is now done only on-demand instead of during startup
- [CHG] Petalas-Aziz pressure drop model now uses a native library by the authors

Version 2.1 Build 4605

- [NEW] Simultaneous Adjust Solver
- [NEW] Added the option to edit pure compound properties through the Pure Compound Properties utility
- [NEW] Added automatic calculation of PR and SRK Peneloux volume translation (shift) coefficients for pseudocomponents
- [FIX] Several fixes to the Pipe Segment (and pressure drop) model, includes Joule-Thomson cooling option
- [FIX] Fixed drawing of Adjust's line connectors

Version 2.1 Build 4602

- [NEW] Added Peneloux volume translation support for PR and SRK Property Packages (Configure Property Package > General Options > Use Peneloux Volume translation => set to 1)
- [FIX] Fixed material stream composition editing in commmand line mode
- [FIX] Fixed ambient temperature not being set on Thermal Profile Editor (Pipe Segment)

Version 2.1 Build 4589

- [CHG] Enabled calculation of material streams when there is no mass/mole flow
- [FIX] Fixed mixer calculation when some of the inlet streams have no flow
- [FIX] Fixed a bug with the mass balance in the gas-liquid separator

Version 2.1 Build 4569

- [NEW] Save/Restore simulation states
- [NEW] Capture flowsheet snapshot and send to clipboard
- [FIX] Fixed command line run mode
- [FIX] Fixed heat capacity coefficients generated by the Compound Creator
- [FIX] Minor fixes to petroleum charact. utilities

Version 2.1 Build 4534

- [FIX] More fixes and enhancements to the flash algorithms
- [FIX] Fixed a bug in the excel interface
- [NEW] Added "reset settings" functionality by pressing Shift during startup

Version 2.1 Build 4526

- [NEW] Added Lua scripting support
- [FIX] Further fixes and improvements to the flash algorithms
- [FIX] Fixed ChemSep column operation with pseudocomponents/hypotheticals
- [FIX] Fixed Zc/Vc calculation for pseudocomponents
- [FIX] Fixed some bugs in the CAPE-OPEN Thermo interfaces

Version 2.1 Build 4513

- [FIX] Enhanced stability and reliability for Pressure-Enthalpy flash calculations

Version 2.1 Build 4503

- [NEW] FPROPS Property Package (needs testing)
- [NEW] Gibbs Minimization (experimental stage!), Hybrid Nested Loops / Inside Out flash algorithms
- [NEW] Added convergence information (error value, time taken and iteration count) for all flash algorithms
- [FIX] Fixed IO PH/PS flash calculation for single compounds
- [FIX] Fixed Property Grid issues with some unit operations and Spanish language
- [FIX] Fixed a bug with energy stream connection to rigorous columns

Version 2.1 Build 4466

- [NEW] Added custom object ordering feature to the Master Table
- [NEW] Added more mole flow units of measure
- [NEW] Added the possibility of removing multiple compounds from the simulation at once
- [FIX] Fixed ChemSep database registry search
- [FIX] Minor translation fixes

Version 2.1 Build 4463

- [FIX] Fixed calculation of ideal gas heat capacity for petroleum fractions

Version 2.1 Build 4452

- [NEW] Petroleum Assay Manager (store/reload/import/export bulk and distillation characterization assay data)
- [NEW] Master Property Table object ordering by property or name ascending/descending
- [NEW] PFD Zoom All, Pan (Shift + Left mouse button)
- [NEW] Area unit conversion to the Heat Exchanger
- [FIX] Fixed PFD Select and Center Object

Version 2.1 Build 4442

- [NEW] Added the Master Property Table to display grouped properties from objects of the same type
- [NEW] Added save-to-image flowsheet feature
- [NEW] Added bubble and dew points to Material Stream's property list
- [NEW] Added the capability of editing custom system of units
- [NEW] Added a keyboard shortcut (Ctrl+E) to edit material stream compositions
- [NEW] Added the capability of showing/hiding table items from the Property Grid

Version 2.1 Build 4438

- [CHG] Changed Adjust variables' units to match the ones in the selected system of units
- [FIX] Fixed custom system of units not being restored
- [FIX] Fixed Adjust behavior
- [FIX] Fixed Flowsheet printing
- [FIX] Fixed Steam Tables Property Package PVF/TVF stream spec calculation
- [FIX] Fixed Bubble and Dew points not being shown if the stream state is single phase
- [FIX] Fixed WFP donation window always showing
- [FIX] Fixed duplicate Separator Op in the Object Palette

Version 2.1 Build 4422

- [CHG] Overall speed and usability improvements
- [NEW] Added drag-and-drop support for opening simulations/cases from Windows Explorer

Version 2.1 Build 4416

- [CHG] Includes ChemSep Lite 6.90, now with 400+ compound database and 40 compounds/300 stages Column model
- [NEW] Includes PRSV2 Property Package with Van Laar-type mixing rule (original PRSV2 w/ Margules MR is now PRSV2-M)
- [FIX] Fixed PRSV2 compresibility factor calculation

Version 2.1 Beta Build 4410

- [NEW] New Property Package: Peng-Robinson-Stryjek-Vera 2 (PRSV2)
- [CHG] Added missing data estimation feature to the Data Regression Utility
- [CHG] Enhanced Unit Set Creator to start with units from the current system

Version 2.1 Beta Build 4407

- [CHG] Added LLE support and initial estimates calculation using UNIFAC structure to the Data Regression Utility
- [CHG] Changed density input from Specific Gravity to API Gravity on the Dist. Curves Characterization Utility
- [FIX] Fixed solver not recalculating outlet streams
- [FIX] Fixed infinite volumetric flow on first Material Stream calculation through the property grid
- [FIX] Fixed Adjust and Spec Ops not showing anything on the property grid

Version 2.1 Beta Build 4404

- [NEW] Added the Compound Creator Utility and corresponding user database structure (work in progress)
- [NEW] Added a Binary Data Regression utility (work in progress)
- [NEW] Added feature request #3409646 (Name given to Material Objects)
- [CHG] Rewritten solver logic increases calculation speed by 35% (Cavett sample)
- [FIX] Fixed Lee-Kesler Cp/Cv calculation
- [FIX] Fixed Distillation Curves Petroleum Characterization Utility
- [FIX] Fixed Pipe Segment model overall HTC calculation and liquid phase volume retrieval
- [FIX] Fixed Spanish translation of the Object Palette
- [FIX] Fixed bug #3409641 (Call to Terminate missing)
- [FIX] Fixed bug #3409637 (Collection Count method called twice in a row)
- [FIX] Fixed bug #3409628 (ICapeUtilities queried twice in a row)

Version 2.0 Build 4258

- [FIX] Fixed unit conversion from lbmol/h to mol/s
- [FIX] Fixed empty compound list array when adding DWSIM Property Packages to CAPE-OPEN simulators
- [FIX] Fixed calculation of activity coefficient when requested from external components (Excel/CAPE-OPEN)
- [FIX] Fixed Energy Stream parameter list (CAPE-OPEN)

Version 2.0 Build 4252

- [FIX] Fixed Compressor and Expander models for single component simulations
- [FIX] Fixed a bug that caused DWSIM to throw an exception on startup related to user-created unit systems
- [FIX] Fixed the negative temperature input bug on Heater and Cooler models

Version 2.0 Build 4251

- [FIX] Fixed the reference state for enthalpy and entropy calculations

Version 2.0 Build 4249

- [NEW] Excel Interface for Equilibrium and Property calculators
- [FIX] Minor bug fixes

Version 2.0 Build 4235

- [FIX] Fixed Shell and Tube Heat Exchanger shell side pressure drop calculation
- [NEW] Added an option to save simulations with password protection
- [CHG] Centralized flowsheet drawing surface (PFD)

Version 2.0 Build 4225

- [FIX] Fixed Shell and Tube Heat Exchanger model
- [FIX] Fixed Units of measure in Watch Panel
- [CHG] Changed update checker to show only a non-obtrusive link on the status panel

Version 2.0 Build 4220

- [NEW] Added a set of shapes for CAPE-OPEN Unit Operations in the flowsheet
- [FIX] Fixed CAPE-OPEN compliancy of the Steam Tables Property Package 
- [FIX] Corrected CAPE-OPEN Report Window behavior

Version 2.0 Build 4207

- [NEW] Support for CAPE-OPEN Unit Operations, Property Packages (1.0/1.1) and Plugins (Flowsheet Monitoring Objects)
- [NEW] DWSIM Property Packages can now be exposed to external CAPE-OPEN compliant simulators
- [NEW] Inside-Out Three-Phase (VLLE) Flash Algorithm
- [NEW] PC-SAFT (without association term) Property Package
- [NEW] UNIFAC Property Package with Liquid-Liquid interaction parameters
- [NEW] Liquid-Liquid Extractor operation mode for the Absorption Column
- [NEW] Three-Phase separation mode for the Vessel
- [NEW] ChemSep database automatic loading
- [NEW] Watch window - allows property monitoring from different objects at the same time
- [NEW] CAPE-OPEN Unit Reports window - view output from CAPE-OPEN Unit Operations
- [NEW] Updated flowsheet drawing theme
- [NEW] 'Send Error Info' button added to the Unhandled Exception window
- [NEW] New version checking tool - informs the user when a new version becomes available and downloads the setup file
- [FIX] General bug fixes and speed improvements

Version 1.8 Build 4101

- [NEW] 'Fouling Factor' calculation mode for the Heat Exchanger Shell and Tube model
- [NEW] Added non-linear solver IPOPT to the Optimizer
- [NEW] DWSIM now reads experimental liquid density and liquid thermal conductivity data for ChemSep components (enabled by default)
- [NEW] Added multiple selection capability to the flowsheet to enable moving multiple objects at once
- [NEW] Added a 'Snap to Grid' capability to the flowsheet for better object alignment
- [CHG] Reactivated flowsheet navigation through the arrow keys
- [CHG] Reactivated the quick connect tool on the flowsheet
- [CHG] General Heat Exchanger model improvements
- [FIX] Fixed 'lbmol/h' unit conversion from SI to English

Version 1.8 Build 4080

- [NEW] Model for rating Shell and Tube Heat Exchangers
- [NEW] Scripting support for pre- and post- Unit Op calculations
- [NEW] Console Output and Calculation Queue windows
- [NEW] "Fast mode" switch for Inside-Out Flash calculations

Version 1.8 Build 3947

- [NEW] Paste from Excel function (Ctrl-V) added to the Composition Editor
- [FIX] Fixed liquid density and water content calculations in Peng-Robinson IWVT Property Package
- [FIX] Fixed Grayson-Streed fugacity calculation
- [CHG] Added ChemSep component support to the COSMO-SAC Property Package

Version 1.8 Build 3938

- [NEW] New converter in Property Grid for some units (temperature, pressure, flow rates, etc.)
- [NEW] New gauge pressure units
- [FIX] Fixed a cut/paste bug in the script editor
- [FIX] Fixed wrong molar/mass fraction values in flowsheet tables

Version 1.8 Build 3922

- [NEW] COSMO-SAC Property Package based on the JCOSMO library (http://code.google.com/p/jcosmo/)
- [CHG] Improvements to the Custom UO script editor
- [CHG] Updated Plugin Interface definition

Version 1.8 Build 3908

- [NEW] Added IronPython, IronRuby, VBScript and JScript scripting support
- [NEW] Added a new Unit Operation: Custom UO, which lets the user run scripts as an unit operation calculation routine

Version 1.7 Build 3875

- [NEW] Added German translation by Rainer Göllnitz

Version 1.7 Build 3868

- [NEW] Interface definition for external plugins
- [FIX] Updated Rigorous Column solvers (Inside-Out and Simultaneous Correction)
- [FIX] Updated Critical Point calculation

Version 1.7 Build 3850

- [NEW] Lee-Kesler-Plöcker Property Package
- [FIX] Fixed K-value calculation call in the Sum Rates method for solving Absorption Columns
- [FIX] Fixed IO Flash calculation in single phase region
- [FIX] Fixed Critical Point calculation with PR and SRK Equations of State
- [FIX] Fixed portions of GUI language that were not being set on the first run

Version 1.7 Build 3840

- [NEW] Gibbs Reactor model (vapor phase only) with two solving methods: reaction extents and direct minimization
- [NEW] New global settings for Property Packages: Flash Algorithm and Calculate Bubble/Dew points
- [NEW] New approach for equilibrium calculation: Inside-Out by Boston and Britt
- [NEW] Added an option to adjust Rackett Parameters and Acentric Factors to match SG and NBP in Petroleum Characterization Utilities
- [NEW] New Quick Settings toolbar: Unit system and number formatting
- [NEW] New menu gives quick access to DWSIM Tools
- [NEW] Added the option to select stream component amounts as properties to show on PFD tables
- [CHG] Completely rewritten Equilibrium Reactor model
- [CHG] Updated ChemSep database loading code to support ChemSep 6.62
- [FIX] Fixed an exception when working with pump curves to calculate pump power
- [FIX] Fixed a bug in temperature calculation with the Steam Tables Property Package

Version 1.6 Build 3756

- [FIX] Fixed Conversion Reactor calculation with more than one reaction in parallel
- [FIX] Fixed Separator Vessel calculation under normal flash settings (Force PH flash option not selected)

Version 1.6 Build 3752

- [NEW] Component Separator Unit Op
- [NEW] Orifice Plate Unit Op
- [CHG] Improvements to the Optimizer, new solvers, more control options
- [CHG] Changed object insertion method from strip buttons to menu items, enhanced object palette to include new view modes
- [FIX] Fixed problems with rigorous column stream connections
- [FIX] Fixed Pump curves feature
- [FIX] Fixed Shortcut Column unhandled exception

Version 1.6 Build 3676

- [NEW] Added new rigorous column specification options
- [NEW] New rigorous column solving method: Napthali-Sandholm Simultaneous Correction (SC)
- [NEW] Added curves support to the Pump unit op
- [NEW] Added expression support to the Sensitivity Analysis Utility
- [FIX] Corrected Cooler and Heater heat exchanged sign
- [CHG] Various Flowsheet improvements (new font and color scheme, new table style, calculation indicator, improved connector drawing algorithm) 
- [CHG] General improvements to the IO Method, new configuration options
- [CHG] Added co/countercurrent flow direction selector to the Heat Exchanger
- [FIX] Fixed Pipe heat loss calculations
- [FIX] Fixed some translation errors
- [FIX] Removed degree symbol from Celsius temperature unit to improve compatibility with foreign languages
- [FIX] Fixed PH Flash setting in Thermo & Reactions configuration section not being effective
- [FIX] Optimizer/Sensitivity utilites: Fixed Material Streams' molar and volumetric flow changes not being effective
- [FIX] Fixed temperature calculations in the Steam Tables property package
- [FIX] Fixed Heat Exchanger Area calculation
- [FIX] Fixed Pipe properties not showing when the GUI language is set to Spanish

Version 1.6 Build 3618

- [FIX] Fixed control placement in the composition editor when using Spanish locale settings

Version 1.6 Build 3605

- [NEW] Added Spanish GUI translation (many thanks to Abad Lira and Gustavo León!)
- [NEW] Added a Multivariate, Constrained Optimization utility
- [NEW] Added a Sensitivity Analysis utility supporting up to 2 independent variables
- [NEW] Added "command-line run mode" (read the documentation for more details)
- [NEW] Added a 'Write to the Flowsheet' capability to the Spreadsheet
- [NEW] New Property Packages: Chao-Seader, Grayson-Streed, Modified UNIFAC (Dortmund) and Peng-Robinson with support for immiscible water and Volume Translation
- [NEW] New Energy Recycle unit operation
- [NEW] Added more units for the most commom properties (temperature, pressure, etc.)
- [NEW] Added code to display a message when the flash algorithm converges to the trivial solution
- [CHG] Redesigned UNIFAC group structure to include all available groups
- [CHG] Report can now show all properties from all objects in the flowsheet
- [CHG] Removed single-phase and phase change limitations from the Heat Exchanger
- [CHG] Property Package selection interface now groups packages by type
- [CHG] Changed component selection interface
- [FIX] Fixed bugs and made minor changes to the code/interface

Version 1.5 Build 3399

- [FIX] Corrected Ideal Gas Enthalpy/Entropy calculation for CheResources and ChemSep database components
- [FIX] Corrected heat duty sign in Equilibrium Reactor
- [FIX] Corrected a unit inconsistency in Rigorous Column heat balances

Version 1.5 Build 3398

- [NEW] Added component volumetric fraction / volumetric flow information to Material Streams
- [NEW] Added a liquid density calculation mode to EOS-specific Property Packages (EOS or Rackett) - this also affects partial volume calculations
- [FIX] Fixed bug #2750848 (null reference error when adding components)
- [FIX] Fixed the 'Recalculate All' calculator feature - it wasn't recalculating input streams
- [FIX] Fixed some errors in the Equilibrium Reactor calculation routine
- [FIX] Fixed some errors when connecting product streams to Absorbers
- [FIX] Added a default value of zero for the UNIFAC groups in the Component Creator to avoid null reference errors

Version 1.5 Build 3377

- [NEW] New Calculator features: Break Calculation, Recalculate All and Clear Queue List
- [NEW] Added information about calculation time
- [FIX] Fixed positioning of rigorous column connections
- [FIX] Fixed UNIFAC Property Package configuration error
- [FIX] Fixed rigorous column 'Decalculate' routine
- [FIX] Fixed stability curve (phase envelope utility) calculation issues
- [FIX] Rigorous columns are now recalculated when editing properties in modal windows
- [CHG] Changed calculator behavior so the interface is more responsive

Version 1.5 Build 3372

- [FIX] Object selection by mouse dragging now updates the property grid correctly
- [FIX] Changed liquid viscosity mixing rule (for multicomponent systems)
- [FIX] Fixed an error in liquid mixture surface tension calculation
- [FIX] Fixed null Viscosity error in C7+ characterization tool
- [FIX] Fixed PRLK Property Package configuration error
- [FIX] Fixed a bug in rigorous column condenser connections
- [FIX] Fixed n-Butane database parameter (liquid viscosity)
- [FIX] Fixed rigorous column full reflux condenser behavior
- [FIX] Fixed vessel and tank null object reference error
- [CHG] Speed improvements in column IO method

Version 1.5 Build 3353

- [NEW] Redesigned component database system
- [NEW] Added two new Property Packages: NRTL/Peng-Robinson and UNIQUAC/Peng-Robinson
- [NEW] Added support for loading ChemSep(TM) databases
- [NEW] Added a tool to characterize petroleum fractions from ASTM/TBP distillation curves
- [NEW] Added a tool to insert user-defined components
- [NEW] Added the ability to use multiple property packages in a single simulation
- [NEW] Dockable help window with localized tips
- [NEW] New Unit Operations: Distillation Column, Absorption Column, Refluxed Absorber and Reboiled Absorber
- [NEW] Rigorous Column solving methods: Bubble-Point, Sum-Rates and Inside-Out
- [NEW] New utility for calculation of Petroleum Cold Flow Properties
- [CHG] Surface Tension is now correctly listed as a liquid phase property
- [CHG] Redesigned splash and welcome screens
- [FIX] Fixed high-pressure vapor viscosity calculation
- [FIX] Fixed liquid viscosity calculation error when supercritical components are present
- [FIX] Fixed flash calculation vapor fraction initialization error (should avoid some impossible solutions)
- [FIX] Fixed culture-specific error related to Property Package parameter storage
- [FIX] Fixed calculation of vapor phase thermal conductivity
- [FIX] Fixed a bug in internal calculation of heat capacity ratio
- [FIX] Fixed MRU filelist

Version 1.4

- [NEW] Added three new Unit Operations: Shortcut Column, Equilibrium Reactor and a basic Heat Exchanger
- [NEW] Added three new utilities: PSV/Vessel Sizing and Spreadsheet
- [CHG] New drag-and-drop feature for adding objects to flowsheet
- [CHG] Some cosmetic changes in the PFD
- [CHG] The critical point utility was modified to calculate multiple critical points when they exists. It's (still) not perfect, but it should work well in most cases.
- [CHG] Petroleum Characterization Utility: temperature-dependent properties are now calculated only when requested, greatly improving the speed of the pseudocomponent creation process.
- [FIX] Corrected the PFD object numbering bug

Version 1.3

- This version is the first to be released under the GPL v3 license
- Added support for English language and translation. Contact the developer for more information.
- Added reactions (conversion, kinetic, equilibrium) and reactors support (Conversion, PFR, CSTR)
- Removed Krypton Toolkit controls in order to mantain consistency of the GUI as a whole
- English reporting now works correctly

- Corrected many other bugs from the previous version