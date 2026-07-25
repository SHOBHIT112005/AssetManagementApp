# ruff: noqa
# Copyright 2026 Google LLC
#
# Licensed under the Apache License, Version 2.0 (the "License");
# you may not use this file except in compliance with the License.
# You may obtain a copy of the License at
#
#     https://www.apache.org/licenses/LICENSE-2.0
#
# Unless required by applicable law or agreed to in writing, software
# distributed under the License is distributed on an "AS IS" BASIS,
# WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
# See the License for the specific language governing permissions and
# limitations under the License.

from google.adk.agents import Agent
from google.adk.apps import App
from google.adk.models import Gemini
from google.genai import types
from pydantic import BaseModel, Field
from datetime import datetime


class FilterCondition(BaseModel):
    field: str = Field(description="The property name on the target entity. Asset: 'Type', 'Status', 'Condition', 'AssetName', 'SerialNumber', 'PurchaseDate', 'WarrantyExpiryDate'. Employee: 'Department', 'Designation', 'Status', 'FullName', 'Email', 'DateOfBirth'. AssetAssignment: 'AssignmentDate', 'ReturnDate'. Use Employee.FullName, Employee.Designation, or Employee.Department to filter assignments by employee.")
    operator: str = Field(description="Operator: '==', '!=', '>', '<', '>=', '<=', 'contains', 'is_null', 'is_not_null'")
    value: str = Field(description="The string value to compare against. E.g. AssetType : 'Laptop', 'Desktop', 'Monitor', 'Keyboard', 'Mouse', 'Phone', 'Printer'. AssetCondition : 'New', 'Good', 'NeedsRepair', 'Damaged'. AssetStatus : 'Available', 'Assigned', 'UnderRepair', 'Retired'. EmployeeStatus : 'Active', 'Inactive'. Department : 'IT', 'HR', 'Marketing', 'Finance', 'Operations'. Keep empty if is_null/is_not_null.")


class AgentAction(BaseModel):
    target_entity: str = Field(description="The entity to query: 'Asset', 'Employee', or 'AssetAssignment'")
    filters: list[FilterCondition] = Field(default_factory=list, description="List of filters to apply (AND logic)")
    limit: int = Field(20, description="Max number of rows to return")
    summary: str = Field(description="A helpful conversational summary responding to the user.")


root_agent = Agent(
    name="asset_admin_agent",
    model=Gemini(
        model="gemini-flash-latest",
        retry_options=types.HttpRetryOptions(attempts=3),
    ),
    instruction=(
        "You are an admin assistant for an asset management application. "
        f"TODAY'S DATE IS : {datetime.now().strftime('%Y-%m-%d')}. "
        "Help admins understand asset, employee, and assignment data. "
        "Map their natural language query into the structured AgentAction response.\n\n"

        "--- QUERY MODE RULES ---\n"
        "Determine the target_entity (Asset, Employee, or AssetAssignment) based on what they are asking for. "
        "Create filters based on their constraints. "
        "For example, 'Show all active laptops' -> target_entity: Asset, filters: [{'field': 'Type', 'operator': '==', 'value': 'Laptop'}, {'field': 'Status', 'operator': '==', 'value': 'Available'}]. "
        "'Show assignments for Priya' -> target_entity: AssetAssignment, filters: [{'field': 'Employee.FullName', 'operator': 'contains', 'value': 'Priya'}]. "
        "'Show currently active assignments' -> target_entity: AssetAssignment, filters: [{'field': 'ReturnDate', 'operator': 'is_null', 'value': ''}]. "
        "CRITICAL RULE FOR EMPLOYEES WITH ASSIGNMENTS: If a user asks for 'employees who have an asset assigned' (e.g. 'Get employees in IT department with an assignment'), you must query the 'AssetAssignment' target_entity. Do not query 'Employee' only. Apply their department or designation filters using the 'Employee.' prefix (e.g., field: 'Employee.Department', value: 'IT'). This works because querying AssetAssignment naturally returns the employees who have or had an assignment. "
        "CRITICAL RULE FOR DATES: When asked for relative dates like 'last month', 'recently', or 'past 30 days', "
        "use TODAY'S DATE to calculate the exact 'yyyy-MM-dd' boundaries. You MUST output two separate filters using the '>=' and '<=' operators on the relevant Date field (e.g. AssignmentDate) to form a range! \n\n"

        "If the user asks to modify, update, assign, or delete data, politely inform them that you are currently in read-only mode and can only assist with querying data.\n\n"

        "You MUST keep responses grounded to ONLY the properties described in the FilterCondition field. "
    ),
    # pyrefly: ignore [unexpected-keyword]
    output_schema=AgentAction,
)

app = App(
    root_agent=root_agent,
    name="admin_agent",
)
