import pytest
from pydantic import ValidationError
from admin_agent.agent import FilterCondition, AgentAction

def test_agent_action_valid():
    """Test that a valid AgentAction initializes properly."""
    action = AgentAction(
        target_entity="Asset",
        filters=[
            FilterCondition(field="Type", operator="==", value="Laptop")
        ],
        limit=10,
        summary="Here are the 10 laptops you requested."
    )
    assert action.target_entity == "Asset"
    assert len(action.filters) == 1
    assert action.limit == 10
    assert "laptops" in action.summary

def test_agent_action_missing_required():
    """Test that missing required fields raises a ValidationError."""
    with pytest.raises(ValidationError):
        # Missing 'summary'
        AgentAction(
            target_entity="Employee",
            filters=[]
        )

def test_filter_condition_valid():
    """Test valid FilterCondition properties."""
    filter_obj = FilterCondition(
        field="Asset.Type",
        operator="==",
        value="Laptop"
    )
    assert filter_obj.field == "Asset.Type"
    assert filter_obj.operator == "=="
    assert filter_obj.value == "Laptop"

def test_filter_condition_missing_fields():
    """Test that a FilterCondition missing a required field raises an error."""
    with pytest.raises(ValidationError):
        FilterCondition(
            field="Asset.Type",
            operator="==",
        )

def test_filter_condition_empty_value_is_valid():
    """Test that an empty string value is valid (useful for is_null/is_not_null)."""
    filter_obj = FilterCondition(
        field="ReturnDate",
        operator="is_null",
        value=""
    )
    assert filter_obj.value == ""
