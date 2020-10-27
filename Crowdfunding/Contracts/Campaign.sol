// SPDX-License-Identifier: MIT
pragma solidity ^0.7.0;

// Hybrid approach: we are still in control of the code that gets deployed
// (the user cannot manipulate the code of the contract and deploy himself)
// but we still make the user pay the deployment costs.

// The factory is in charge of deploying new campaigns and it also keeps
// a record of all instances of the Campaign contract that got deployed.
// The user sends a transaction to the factory, along with some amount of
// gas to use for the deployment.
contract CampaignFactory {
    address[] public deployedCampaigns;
    uint public campaignsCount;
    
    function createCampaign(uint minimum) public {
        // Create a new Campaign contract and get its address
        address newCampaign = address(new Campaign(minimum, msg.sender));
        deployedCampaigns.push(newCampaign);
        campaignsCount++;
    }
}

contract Campaign{
    
    struct Request {
        string description;
        uint256 value;
        address payable recipient;
        bool complete;
        uint256 approvalCount;
        mapping(address => bool) approvals;
    }
    
    address public manager;
    uint public minimumContribution;
    
    // Do not use an array for approvers, when we loop over thousands of users
    // we will spend a lot of gas! Mappings are basically a hashtable.
    mapping(address => bool) public contributors;
    uint public contributorsCount;
    mapping(uint => Request) public requests;
    uint public requestsCount;
    
    modifier restricted() {
        require(msg.sender == manager);
        _; // Code of the modified function goes here
    }
    
    // We don't use msg.sender because it would be the addr of the factory!
    constructor(uint minimum, address creator) {
        manager = creator;
        minimumContribution = minimum;
    }
    
    function contribute() public payable {
        require(msg.value > minimumContribution);
        
        contributors[msg.sender] = true;
        contributorsCount++;
    }
    
    function createRequest(string memory description, uint value, address payable recipient) 
        public restricted {
        
        // Set the output variable
        uint requestId = requestsCount++;
        
        // We cannot use "requests[requestId] = Request(description, value, recipient, false, 0)"
        // because the RHS creates a memory-struct "Request" that contains a mapping, which is
        // not allowed in solidity 0.7.
        Request storage newRequest = requests[requestId];
        newRequest.description = description;
        newRequest.value = value;
        newRequest.recipient = recipient;
    }
    
    function approveRequest(uint requestId) public {
        Request storage request = requests[requestId];
        
        require(contributors[msg.sender]);
        require(!request.approvals[msg.sender]);
        
        request.approvals[msg.sender] = true;
        request.approvalCount++;
    }
    
    function finalizeRequest(uint requestId) public restricted {
        Request storage request = requests[requestId];

        // At least half of the people must approve a spending request
        require(request.approvalCount > contributorsCount / 2);
        require(!request.complete);
        
        // Send the money to the recipient
        request.complete = true;
        request.recipient.transfer(request.value);
    }
}