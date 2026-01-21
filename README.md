# Static Analysis – Practical Demonstration (C#)

## Overview

This repository is a **practical implementation** of the concepts explained in my Medium article on **static malware analysis and its visibility limits**.

- The **Medium article** explains *how static analysis works conceptually*
- This **GitHub repository** demonstrates those concepts *in practice*

The purpose is to show **how static analysis evaluates files at rest**, how it infers risk from visible indicators, and where its **visibility naturally ends**.

> This project is strictly educational and defensive.  
> It focuses on understanding detection logic, not bypassing security controls.

---

## Repository Structure

```text
Static-Analysis/
│
├── detected.cs      # Baseline implementation with high static visibility
├── undetected.cs    # Transformed implementation with reduced static visibility
└── README.md
```

---

## Background: What Static Analysis Sees

Static analysis inspects a file **without executing it**.

When a file is written to disk, security engines may analyze:
- Embedded strings and raw data
- Imported APIs and libraries
- File structure and metadata
- Known signatures and byte patterns

All decisions are made **before execution**, based solely on **what is visible inside the file at rest**.

Static analysis does **not**:
- Execute code
- Observe runtime-generated values
- Understand dynamically constructed logic

This repository demonstrates that exact limitation.

---

## How the Code Works (Common Execution Flow)

Both `detected.cs` and `undetected.cs` follow the **same runtime execution flow**:

1. Prepare a payload in memory  
2. Allocate writable memory using Windows APIs  
3. Copy the payload into allocated memory  
4. Change memory permissions to executable  
5. Execute the payload using a new thread  
6. Wait for execution to complete  

**The runtime behavior is identical in both files.**  
What differs is **what static analysis can see before execution**.

---

## File 1: `detected.cs`  Baseline (High Static Visibility)

### How the Code Works

- The payload is stored **directly inside the binary** in plaintext form.
- Windows API functions for memory allocation and execution are imported.
- The execution flow is clearly inferable from static inspection.

### Why Static Analysis Detects This File

Because everything is visible **at rest**, static analysis engines can:
- Extract and scan the payload
- Recognize high-risk API combinations
- Infer in-memory execution behavior
- Classify the file before execution

This is static analysis working **as designed**.

---

## File 2: `undetected.cs`  Transformed (Reduced Static Visibility)

### What Changed Compared to `detected.cs`

The **execution flow did not change**.  
Only **static visibility changed**.

- The payload is no longer stored in plaintext.
- Meaningful data is reconstructed only during execution.
- Static analysis cannot observe runtime-generated values.

### Why Static Analysis Interpretation Changes

Static analysis relies on **what it can see on disk**.

When meaningful data is not directly visible:
- Pattern matching becomes ineffective
- Risk scoring changes
- Static classification may not trigger

This does **not** mean detection is bypassed.
It demonstrates the **visibility boundary of static analysis**.

---

## What This Demonstration Proves

- Static analysis evaluates **visibility**, not behavior
- Identical runtime behavior can appear different at rest
- Static analysis is only one detection layer

---

## Compilation Instructions (Required)

### Environment

- Windows
- Visual Studio
- Console App (.NET Framework)

### Build Steps

1. Create a **Console App (.NET Framework)** project
2. Replace `Program.cs` with either:
   - `detected.cs`, or
   - `undetected.cs`
3. Set:
   - **Configuration:** Release
   - **Platform:** x64
4. Build the project

### Why x64 Matters

- The code relies on a 64-bit execution context
- Memory layout assumptions are x64-specific

---

## Defensive & Educational Scope

This repository is intended for:
- Security students
- Blue teamers
- Malware analysts
- Detection engineers

It exists to **understand detection decisions**, not defeat security systems.

---

## Key Takeaway

Static analysis is not weak.
It is **purpose-built**.

It evaluates **what is visible at rest**.
When visibility changes, interpretation changes  even if behavior does not.

---

## Disclaimer

All examples are provided **solely for educational purposes**.
