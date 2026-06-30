# TODO

--------------------------------------------------------------------------------
## 0. GitHub Copilot: Potential bugs and incoherences by COPILOT GPT-4.1

--------------------------------------------------------------------------------
### - [_] 012 -- Test coverage: The tests do not cover all functionality, especially error handling and edge cases. More comprehensive tests are needed to ensure robustness.

--------------------------------------------------------------------------------
## 1. GitHub Copilot: Here are the main bugs, inconsistencies, and areas for improvement I found in your code by o4-mini (Preview)

--------------------------------------------------------------------------------
### - [_] 1010. Tests don't cover core functionality  
    - No tests for `ConvertToIcon`, `ProcessDirectory`, or error paths.  
    - Suggestion: add unit/integration tests for image conversion and CLI argument parsing.

--------------------------------------------------------------------------------
## 2. GitHub Copilot: Here are the main bugs, inconsistencies, and areas for improvement I found in your code by Claude 3.7 Sonnet

--------------------------------------------------------------------------------
### - [_] 2010. Incomplete test coverage: There are no tests for the main functionality of the program (e.g., ProcessDirectory, IconConversion).

--------------------------------------------------------------------------------
## 3. GitHub Copilot: Here are the main bugs, inconsistencies, and areas for improvement I found in your code by Gemini 2.5 Pro (Preview)

--------------------------------------------------------------------------------

--------------------------------------------------------------------------------
## 9. Unit Testing Requirements

This project requires comprehensive unit tests to ensure functionality and maintainability. 
The following test cases should be implemented:

- Cover core functionality including directory processing and image conversion
- Test error handling paths for invalid inputs and edge cases
- Verify command-line argument parsing logic
- Validate language detection functionality
- Ensure proper image conversion across various formats
- Test directory traversal and processing
- Verify desktop.ini file creation and formatting
- Test icon cache refresh functionality for different operating systems

Prioritize tests that verify the main application workflow and critical error paths.

--------------------------------------------------------------------------------
### - [x] 9000. Change terminal to be able to see Chinese characters. (e.g., chcp 65001)

--------------------------------------------------------------------------------
### - [_] 9001. Add tests for the main functionality of the program (e.g., ProcessDirectory, IconConversion).

--------------------------------------------------------------------------------
### - [_] 9002. Add tests for the error handling paths (e.g., invalid image formats, file not found).

--------------------------------------------------------------------------------
### - [_] 9003. Add tests for the command-line argument parsing.

--------------------------------------------------------------------------------
### - [_] 9004. Add tests for the language detection logic.

--------------------------------------------------------------------------------
### - [_] 9005. Add tests for the image conversion logic.

--------------------------------------------------------------------------------
### - [_] 9006. Add tests for the directory processing logic.

--------------------------------------------------------------------------------
### - [_] 9007. Add tests for the desktop.ini file creation logic.

--------------------------------------------------------------------------------
### - [_] 9008. Add tests for the icon cache refresh logic.

--------------------------------------------------------------------------------
