# Overview

The Thermo Raw Metadata Plotter can be used to make X vs. Y plots of metadata in a Thermo .raw file.
Supported metadata types include:
* Scan Number
* MS Level
* Retention Time
* Ion Injection Time (ms)
* Base Peak Intensity
* Total Ion Current

Any of these metrics can be plotted against one another on an XY plot.
The plot can be customized to only show MS1 data or MS2 data or all MSn data.

## Example Plots

The `ExampleData` directory has several example plots producing using the Thermo .raw file in that directory.

## Data Export

The extracted metadata can be exported as a tab-delimited text file or as a CSV file. For example:

| Scan Number | MS Level | Retention Time | Ion Injection Time (ms) | BPI | TIC |
|-------------|----------|----------------|-------------------------|-----|-----|
| 1  | 1 | 0.002 | 0.936 | 494163904  | 1548210048 |
| 2  | 1 | 0.006 | 0.012 | 1187917056 | 3362984960 |
| 3  | 2 | 0.006 | 0.467 | 248567808  | 1699406336 |
| 4  | 2 | 0.009 | 0.48  | 161390784  | 1047341056 |
| 5  | 2 | 0.010 | 7.157 | 7202350.5  | 108458864  |
| 6  | 2 | 0.014 | 0.448 | 269340608  | 1600375296 |
| 7  | 2 | 0.016 | 0.448 | 256003216  | 1543565568 |
| 8  | 2 | 0.018 | 0.448 | 240610640  | 1437129984 |
| 9  | 2 | 0.020 | 0.459 | 143611568  | 763128192  |
| 10 | 1 | 0.022 | 0.16  | 778636160  | 2344517632 |
| 11 | 2 | 0.024 | 0.425 | 271784288  | 1855613184 | 
| 12 | 2 | 0.026 | 0.43  | 190608896  | 1249527680 |

## Contacts

Written by Bryson Gibbons for the Department of Energy (PNNL, Richland, WA) \
E-mail: bryson.gibbons@pnnl.gov or proteomics@pnnl.gov \
Website: https://omics.pnl.gov/ or https://panomics.pnnl.gov/

## License

Licensed under the 2-Clause BSD License; you may not use this file except 
in compliance with the License.  You may obtain a copy of the License at 
https://opensource.org/licenses/BSD-2-Clause

Copyright 2018 Battelle Memorial Institute

RawFileReader reading tool. Copyright © 2016 by Thermo Fisher Scientific, Inc. All rights reserved.
