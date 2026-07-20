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

import os
from dotenv import load_dotenv

# Load environment variables from .env file so the API key is available
load_dotenv()

from google.adk.agents.run_config import RunConfig, StreamingMode
from google.adk.runners import Runner
from google.adk.sessions import InMemorySessionService
from google.genai import types

from admin_agent.agent import root_agent

def test_agent_structured_output() -> None:
    """
    Integration test to verify the agent returns the correct AgentAction schema.
    """
    session_service = InMemorySessionService()
    session = session_service.create_session_sync(user_id="test_user", app_name="test")
    runner = Runner(agent=root_agent, session_service=session_service, app_name="test")

    message = types.Content(
        role="user", parts=[types.Part.from_text(text="Show me all laptops")]
    )

    events = list(
        runner.run(
            new_message=message,
            user_id="test_user",
            session_id=session.id,
        )
    )

    assert len(events) > 0, "Expected at least one event from the agent"
    final_output = events[-1].data
    assert final_output.target_entity == "Asset"

def test_agent_employee_assignment_rule() -> None:
    """Test 'Get employees in IT department with an assignment' routes to AssetAssignment."""
    session_service = InMemorySessionService()
    session = session_service.create_session_sync(user_id="test_user", app_name="test")
    runner = Runner(agent=root_agent, session_service=session_service, app_name="test")

    message = types.Content(
        role="user", parts=[types.Part.from_text(text="Get employees in IT department with an assignment")]
    )

    events = list(runner.run(new_message=message, user_id="test_user", session_id=session.id))
    final_output = events[-1].data
    
    assert final_output.target_entity == "AssetAssignment", "Should route to AssetAssignment based on CRITICAL RULE."
    # Expect a filter for Employee.Department
    it_filter = next((f for f in final_output.filters if f.field == "Employee.Department" and f.value == "IT"), None)
    assert it_filter is not None, "Missing Employee.Department filter."

def test_agent_relative_dates() -> None:
    """Test 'Show laptops added last month' splits into >= and <= date filters."""
    session_service = InMemorySessionService()
    session = session_service.create_session_sync(user_id="test_user", app_name="test")
    runner = Runner(agent=root_agent, session_service=session_service, app_name="test")

    message = types.Content(
        role="user", parts=[types.Part.from_text(text="Show laptops added last month")]
    )

    events = list(runner.run(new_message=message, user_id="test_user", session_id=session.id))
    final_output = events[-1].data
    
    assert final_output.target_entity == "Asset"
    # Find a date filter
    date_filters = [f for f in final_output.filters if 'Date' in f.field]
    assert len(date_filters) >= 2, "Should split relative date into start and end boundaries."
    operators = [f.operator for f in date_filters]
    assert '>=' in operators and '<=' in operators, "Must use >= and <= for date ranges."

def test_agent_readonly_mode() -> None:
    """Test the agent refuses to modify data."""
    session_service = InMemorySessionService()
    session = session_service.create_session_sync(user_id="test_user", app_name="test")
    runner = Runner(agent=root_agent, session_service=session_service, app_name="test")

    message = types.Content(
        role="user", parts=[types.Part.from_text(text="Assign a new laptop to John")]
    )

    events = list(runner.run(new_message=message, user_id="test_user", session_id=session.id))
    final_output = events[-1].data
    
    # Should contain polite refusal in summary
    assert "read-only" in final_output.summary.lower() or "cannot" in final_output.summary.lower() or "only assist" in final_output.summary.lower()
