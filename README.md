# InternLoop

**InternLoop** is a CLI tool that orchestrates AI coding agents (Cursor and GitHub Copilot) to build software applications using a component-based, plan-driven workflow. You describe what you want, and the AI agents implement it step by step—from high-level architecture to individual components.

---

## Summary

InternLoop automates the software development lifecycle by:

1. **Creating plans** – Turns your natural-language description into a structured, component-based architecture (`.ai/plan.json`)
2. **Building projects** – Orchestrates AI agents to scaffold an empty project and implement each component in dependency order
3. **Developing features** – Handles iterative changes by creating change plans and implementing updates to existing components

The tool uses **Cursor** or **GitHub Copilot** as the underlying AI agent, so you can switch between them depending on your setup or preference. Plans are stored as JSON, making the process transparent and editable.

---

## Why This Is Useful

- **Structured AI development** – Instead of one-shot prompts, InternLoop breaks work into an architecture, components, and dependencies, so the AI stays focused and consistent.
- ** Component-based architecture** – Enforces a modular design: low-level components first, then higher-level ones composed from them.
- **Agent flexibility** – Use Cursor or Copilot from the same workflow; switch agents without changing your process.
- **Reproducible builds** – Plans are versionable and can be reused or shared across machines.
- **Change management** – `develop` keeps your architecture in sync by creating change plans and merging updates into the main plan.

---

## Prerequisites

- [.NET 8 or later](https://dotnet.microsoft.com/download)
- **Cursor** or **GitHub Copilot** CLI installed and configured
- Run from the project directory where you want to build the application

---

## How to Run

```bash
# Run interactively (prompts for commands)
dotnet run

# Run a single command
dotnet run -- create-plan "A todo app with categories and due dates"

# Run with arguments
dotnet run -- develop "Add email notifications"
```

---

## Commands

### `create-plan "<description>"`

Creates a new architecture plan from your description. The AI generates:

- Application description
- Architectural decisions
- A list of components with dependencies, descriptions, and detailed designs

**Output:** `.ai/plan.json` (or equivalent, depending on agent output)

**Examples:**

```bash
dotnet run -- create-plan "A blog with comments and tags"
dotnet run -- create-plan "A REST API for inventory management"
```

---

### `build`

Builds the project from the current plan:

1. If all components are `NotStarted`, creates an empty project scaffold
2. Builds components in dependency order (no dependencies first, then dependents)
3. Updates `plan.json` with completion status after each component

**Requires:** `plan.json` in the current directory

**Note:** The `build` command expects `plan.json` in the project root; some agents may output to `.ai/plan.json`. Move or symlink as needed.

```bash
dotnet run -- build
```

---

### `develop "<description>"`

Implements a feature or fix on top of an existing project:

1. Creates a change plan in `.ai/change-plan.json` based on your description
2. Implements each component in the change plan
3. Updates `.ai/plan.json` with new or modified components

**Requires:** `.ai/plan.json` and `.ai/change-plan.json` (created by the agent)

**Examples:**

```bash
dotnet run -- develop "Add user authentication"
dotnet run -- develop "Fix the search to handle special characters"
```

---

### `switch-agent [cursor|copilot]`

Switches the AI agent or shows the current one.

- **No argument:** Shows current agent and available options
- **With argument:** Switches to the specified agent

**Examples:**

```bash
dotnet run -- switch-agent          # Show current agent
dotnet run -- switch-agent cursor   # Use Cursor
dotnet run -- switch-agent copilot  # Use GitHub Copilot
```

Agent selection is persisted in `config.json` in the app directory.

---

## Plan File Format

Plans are JSON with this structure:

```json
{
  "application_description": "Description of the app",
  "architectural_decisions": ["decision 1", "decision 2"],
  "components": [
    {
      "component_name": "ComponentName",
      "component_description": "What it does",
      "component_detailed_design": "How it works",
      "dependencies": ["OtherComponent1", "OtherComponent2"],
      "development_status": "NotStarted"
    }
  ]
}
```

`development_status` is updated to `Completed` as components are built.

---

## Typical Workflow

1. **Create a plan**
   ```bash
   dotnet run -- create-plan "A task manager with projects and labels"
   ```

2. **Move plan if needed** (if the agent wrote to `.ai/plan.json`)
   ```bash
   mkdir -p .ai && mv plan.json .ai/plan.json
   # Or ensure build reads from .ai/plan.json
   ```

3. **Build the project**
   ```bash
   dotnet run -- build
   ```

4. **Iterate with `develop`**
   ```bash
   dotnet run -- develop "Add dark mode"
   ```

5. **Switch agents if desired**
   ```bash
   dotnet run -- switch-agent copilot
   ```

---

## Project Structure

```
InternLoop/
├── Commands/           # Command implementations
├── Helpers/            # Agent integration (Cursor, Copilot)
├── Models/             # Plan, config, and agent models
├── Program.cs          # Entry point and command loop
└── README.md
```

---

## License

See the repository license file for details.
