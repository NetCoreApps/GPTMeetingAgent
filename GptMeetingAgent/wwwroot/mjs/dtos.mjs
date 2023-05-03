/* Options:
Date: 2023-04-25 10:51:01
Version: 6.80
Tip: To override a DTO option, remove "//" prefix before updating
BaseUrl: https://localhost:5001

//AddServiceStackTypes: True
//AddDocAnnotations: True
//AddDescriptionAsComments: True
//IncludeTypes: 
//ExcludeTypes: 
//DefaultImports: 
*/

"use strict";
export class AgentCommand {
    /** @param {{name?:string,body?:Object}} [init] */
    constructor(init) { Object.assign(this, init) }
    /** @type {string} */
    name;
    /** @type {Object} */
    body;
}
export class QueryBase {
    /** @param {{skip?:number,take?:number,orderBy?:string,orderByDesc?:string,include?:string,fields?:string,meta?:{ [index: string]: string; }}} [init] */
    constructor(init) { Object.assign(this, init) }
    /** @type {?number} */
    skip;
    /** @type {?number} */
    take;
    /** @type {string} */
    orderBy;
    /** @type {string} */
    orderByDesc;
    /** @type {string} */
    include;
    /** @type {string} */
    fields;
    /** @type {{ [index: string]: string; }} */
    meta;
}
/** @typedef T {any} */
export class QueryDb extends QueryBase {
    /** @param {{skip?:number,take?:number,orderBy?:string,orderByDesc?:string,include?:string,fields?:string,meta?:{ [index: string]: string; }}} [init] */
    constructor(init) { super(init); Object.assign(this, init) }
}
export class TeamUser {
    /** @param {{id?:number,displayName?:string,mail?:string,userPrincipalName?:string}} [init] */
    constructor(init) { Object.assign(this, init) }
    /** @type {number} */
    id;
    /** @type {string} */
    displayName;
    /** @type {string} */
    mail;
    /** @type {string} */
    userPrincipalName;
}
export class EmailAddress {
    /** @param {{address?:string,name?:string}} [init] */
    constructor(init) { Object.assign(this, init) }
    /** @type {string} */
    address;
    /** @type {string} */
    name;
}
export class Attendee {
    /** @param {{emailAddress?:EmailAddress,type?:string}} [init] */
    constructor(init) { Object.assign(this, init) }
    /** @type {EmailAddress} */
    emailAddress;
    /** @type {string} */
    type;
}
export class CalendarEntry {
    /** @param {{id?:number,calendarId?:string,subject?:string,body?:string,start?:string,end?:string,attendees?:Attendee[],transactionId?:string}} [init] */
    constructor(init) { Object.assign(this, init) }
    /** @type {number} */
    id;
    /** @type {string} */
    calendarId;
    /** @type {string} */
    subject;
    /** @type {string} */
    body;
    /** @type {string} */
    start;
    /** @type {string} */
    end;
    /** @type {Attendee[]} */
    attendees;
    /** @type {string} */
    transactionId;
}
export class StoredAgentCommand extends AgentCommand {
    /** @param {{id?:number,response?:Object,storedChatMessageId?:number,name?:string,body?:Object}} [init] */
    constructor(init) { super(init); Object.assign(this, init) }
    /** @type {number} */
    id;
    /** @type {Object} */
    response;
    /** @type {number} */
    storedChatMessageId;
}
export class AgentTask {
    /** @param {{taskRef?:string,prompt?:string,successCriteria?:string,completed?:boolean}} [init] */
    constructor(init) { Object.assign(this, init) }
    /** @type {string} */
    taskRef;
    /** @type {string} */
    prompt;
    /** @type {string} */
    successCriteria;
    /** @type {boolean} */
    completed;
}
export class ChatMessage {
    /** @param {{role?:string,content?:string,created?:string}} [init] */
    constructor(init) { Object.assign(this, init) }
    /** @type {string} */
    role;
    /** @type {string} */
    content;
    /** @type {string} */
    created;
}
export class StoredChatMessage extends ChatMessage {
    /** @param {{id?:number,contentIsJson?:boolean,storedAgentTaskId?:number,command?:StoredAgentCommand,role?:string,content?:string,created?:string}} [init] */
    constructor(init) { super(init); Object.assign(this, init) }
    /** @type {number} */
    id;
    /** @type {boolean} */
    contentIsJson;
    /** @type {number} */
    storedAgentTaskId;
    /** @type {?StoredAgentCommand} */
    command;
}
export class StoredAgentTask extends AgentTask {
    /** @param {{id?:number,storedAgentDataId?:number,name?:string,chatHistory?:StoredChatMessage[],taskRef?:string,prompt?:string,successCriteria?:string,completed?:boolean}} [init] */
    constructor(init) { super(init); Object.assign(this, init) }
    /** @type {number} */
    id;
    /** @type {number} */
    storedAgentDataId;
    /** @type {?string} */
    name;
    /** @type {StoredChatMessage[]} */
    chatHistory;
}
export class ScheduleItem {
    /** @param {{id?:number,isPrivate?:boolean,status?:string,subject?:string,location?:string,start?:string,end?:string,attendeeEmails?:string[],teamUserId?:number}} [init] */
    constructor(init) { Object.assign(this, init) }
    /** @type {number} */
    id;
    /** @type {?boolean} */
    isPrivate;
    /** @type {string} */
    status;
    /** @type {string} */
    subject;
    /** @type {string} */
    location;
    /** @type {string} */
    start;
    /** @type {string} */
    end;
    /** @type {string[]} */
    attendeeEmails;
    /** @type {number} */
    teamUserId;
}
export class TimeZoneInfo {
    /** @param {{name?:string,bias?:number,oDataType?:string,standardOffset?:string,daylightOffset?:string,daylightBias?:number}} [init] */
    constructor(init) { Object.assign(this, init) }
    /** @type {string} */
    name;
    /** @type {?number} */
    bias;
    /** @type {string} */
    oDataType;
    /** @type {string} */
    standardOffset;
    /** @type {string} */
    daylightOffset;
    /** @type {?number} */
    daylightBias;
}
export class WorkingHours {
    /** @param {{daysOfWeek?:string[],startTime?:string,endTime?:string,timeZone?:TimeZoneInfo}} [init] */
    constructor(init) { Object.assign(this, init) }
    /** @type {string[]} */
    daysOfWeek;
    /** @type {string} */
    startTime;
    /** @type {string} */
    endTime;
    /** @type {TimeZoneInfo} */
    timeZone;
}
export class ScheduleInformation {
    /** @param {{scheduleId?:string,availabilityView?:string,scheduleItems?:ScheduleItem[],workingHours?:WorkingHours}} [init] */
    constructor(init) { Object.assign(this, init) }
    /** @type {string} */
    scheduleId;
    /** @type {string} */
    availabilityView;
    /** @type {ScheduleItem[]} */
    scheduleItems;
    /** @type {WorkingHours} */
    workingHours;
}
export class TeamChannel {
    /** @param {{id?:number,createdDateTime?:string,displayName?:string,description?:string,membershipType?:string}} [init] */
    constructor(init) { Object.assign(this, init) }
    /** @type {number} */
    id;
    /** @type {string} */
    createdDateTime;
    /** @type {string} */
    displayName;
    /** @type {string} */
    description;
    /** @type {string} */
    membershipType;
}
export class GptAgentData {
    /** @param {{promptBase?:string,name?:string,role?:string}} [init] */
    constructor(init) { Object.assign(this, init) }
    /** @type {string} */
    promptBase;
    /** @type {string} */
    name;
    /** @type {string} */
    role;
}
export class AgentThoughts {
    /** @param {{text?:string,reasoning?:string,plan?:string,criticism?:string,speak?:string}} [init] */
    constructor(init) { Object.assign(this, init) }
    /** @type {string} */
    text;
    /** @type {string} */
    reasoning;
    /** @type {string} */
    plan;
    /** @type {string} */
    criticism;
    /** @type {string} */
    speak;
}
export class ResponseError {
    /** @param {{errorCode?:string,fieldName?:string,message?:string,meta?:{ [index: string]: string; }}} [init] */
    constructor(init) { Object.assign(this, init) }
    /** @type {string} */
    errorCode;
    /** @type {string} */
    fieldName;
    /** @type {string} */
    message;
    /** @type {{ [index: string]: string; }} */
    meta;
}
export class ResponseStatus {
    /** @param {{errorCode?:string,message?:string,stackTrace?:string,errors?:ResponseError[],meta?:{ [index: string]: string; }}} [init] */
    constructor(init) { Object.assign(this, init) }
    /** @type {string} */
    errorCode;
    /** @type {string} */
    message;
    /** @type {string} */
    stackTrace;
    /** @type {ResponseError[]} */
    errors;
    /** @type {{ [index: string]: string; }} */
    meta;
}
export class CreateCalendarEventResponse {
    /** @param {{oDataContext?:string,oDataEtag?:string,id?:string,createdDateTime?:string,lastModifiedDateTime?:string,changeKey?:string,categories?:string[],subject?:string,bodyPreview?:string,isAllDay?:boolean,isCancelled?:boolean,isOrganizer?:boolean,body?:string,start?:string,end?:string,location?:string,attendees?:string[],organizer?:string}} [init] */
    constructor(init) { Object.assign(this, init) }
    /** @type {string} */
    oDataContext;
    /** @type {string} */
    oDataEtag;
    /** @type {string} */
    id;
    /** @type {string} */
    createdDateTime;
    /** @type {string} */
    lastModifiedDateTime;
    /** @type {string} */
    changeKey;
    /** @type {string[]} */
    categories;
    /** @type {string} */
    subject;
    /** @type {string} */
    bodyPreview;
    /** @type {boolean} */
    isAllDay;
    /** @type {boolean} */
    isCancelled;
    /** @type {boolean} */
    isOrganizer;
    /** @type {string} */
    body;
    /** @type {string} */
    start;
    /** @type {string} */
    end;
    /** @type {string} */
    location;
    /** @type {string[]} */
    attendees;
    /** @type {string} */
    organizer;
}
export class SearchUsersResponse {
    /** @param {{value?:TeamUser[]}} [init] */
    constructor(init) { Object.assign(this, init) }
    /** @type {TeamUser[]} */
    value;
}
export class GetUserScheduleResponse {
    /** @param {{oDataContext?:string,value?:ScheduleInformation[]}} [init] */
    constructor(init) { Object.assign(this, init) }
    /** @type {string} */
    oDataContext;
    /** @type {ScheduleInformation[]} */
    value;
}
export class ListChannelsInTeamResponse {
    /** @param {{value?:TeamChannel[]}} [init] */
    constructor(init) { Object.assign(this, init) }
    /** @type {TeamChannel[]} */
    value;
}
export class HelloResponse {
    /** @param {{result?:string}} [init] */
    constructor(init) { Object.assign(this, init) }
    /** @type {string} */
    result;
}
export class ListTasksForAgentResponse {
    /** @param {{tasks?:StoredAgentTask[]}} [init] */
    constructor(init) { Object.assign(this, init) }
    /** @type {StoredAgentTask[]} */
    tasks;
}
export class ListChatHistoryForTaskResponse {
    /** @param {{chatHistory?:StoredChatMessage[]}} [init] */
    constructor(init) { Object.assign(this, init) }
    /** @type {StoredChatMessage[]} */
    chatHistory;
}
export class StoredAgentData extends GptAgentData {
    /** @param {{id?:number,promptBase?:string,name?:string,role?:string}} [init] */
    constructor(init) { super(init); Object.assign(this, init) }
    /** @type {number} */
    id;
}
export class UpdateAgentCommandResponse {
    /** @param {{command?:StoredAgentCommand}} [init] */
    constructor(init) { Object.assign(this, init) }
    /** @type {?StoredAgentCommand} */
    command;
}
export class ContinueGptAgentTaskResponse {
    /** @param {{task?:StoredAgentTask,command?:StoredAgentCommand,thoughts?:AgentThoughts,question?:boolean}} [init] */
    constructor(init) { Object.assign(this, init) }
    /** @type {StoredAgentTask} */
    task;
    /** @type {?StoredAgentCommand} */
    command;
    /** @type {AgentThoughts} */
    thoughts;
    /** @type {boolean} */
    question;
}
export class StartGptAgentTaskResponse {
    /** @param {{task?:StoredAgentTask,thoughts?:AgentThoughts,command?:StoredAgentCommand,question?:boolean}} [init] */
    constructor(init) { Object.assign(this, init) }
    /** @type {StoredAgentTask} */
    task;
    /** @type {AgentThoughts} */
    thoughts;
    /** @type {?StoredAgentCommand} */
    command;
    /** @type {boolean} */
    question;
}
export class AuthenticateResponse {
    /** @param {{userId?:string,sessionId?:string,userName?:string,displayName?:string,referrerUrl?:string,bearerToken?:string,refreshToken?:string,profileUrl?:string,roles?:string[],permissions?:string[],responseStatus?:ResponseStatus,meta?:{ [index: string]: string; }}} [init] */
    constructor(init) { Object.assign(this, init) }
    /** @type {string} */
    userId;
    /** @type {string} */
    sessionId;
    /** @type {string} */
    userName;
    /** @type {string} */
    displayName;
    /** @type {string} */
    referrerUrl;
    /** @type {string} */
    bearerToken;
    /** @type {string} */
    refreshToken;
    /** @type {string} */
    profileUrl;
    /** @type {string[]} */
    roles;
    /** @type {string[]} */
    permissions;
    /** @type {ResponseStatus} */
    responseStatus;
    /** @type {{ [index: string]: string; }} */
    meta;
}
export class AssignRolesResponse {
    /** @param {{allRoles?:string[],allPermissions?:string[],meta?:{ [index: string]: string; },responseStatus?:ResponseStatus}} [init] */
    constructor(init) { Object.assign(this, init) }
    /** @type {string[]} */
    allRoles;
    /** @type {string[]} */
    allPermissions;
    /** @type {{ [index: string]: string; }} */
    meta;
    /** @type {ResponseStatus} */
    responseStatus;
}
export class UnAssignRolesResponse {
    /** @param {{allRoles?:string[],allPermissions?:string[],meta?:{ [index: string]: string; },responseStatus?:ResponseStatus}} [init] */
    constructor(init) { Object.assign(this, init) }
    /** @type {string[]} */
    allRoles;
    /** @type {string[]} */
    allPermissions;
    /** @type {{ [index: string]: string; }} */
    meta;
    /** @type {ResponseStatus} */
    responseStatus;
}
export class RegisterResponse {
    /** @param {{userId?:string,sessionId?:string,userName?:string,referrerUrl?:string,bearerToken?:string,refreshToken?:string,roles?:string[],permissions?:string[],responseStatus?:ResponseStatus,meta?:{ [index: string]: string; }}} [init] */
    constructor(init) { Object.assign(this, init) }
    /** @type {string} */
    userId;
    /** @type {string} */
    sessionId;
    /** @type {string} */
    userName;
    /** @type {string} */
    referrerUrl;
    /** @type {string} */
    bearerToken;
    /** @type {string} */
    refreshToken;
    /** @type {string[]} */
    roles;
    /** @type {string[]} */
    permissions;
    /** @type {ResponseStatus} */
    responseStatus;
    /** @type {{ [index: string]: string; }} */
    meta;
}
/** @typedef T {any} */
export class QueryResponse {
    /** @param {{offset?:number,total?:number,results?:T[],meta?:{ [index: string]: string; },responseStatus?:ResponseStatus}} [init] */
    constructor(init) { Object.assign(this, init) }
    /** @type {number} */
    offset;
    /** @type {number} */
    total;
    /** @type {T[]} */
    results;
    /** @type {{ [index: string]: string; }} */
    meta;
    /** @type {ResponseStatus} */
    responseStatus;
}
export class CreateCalendarEvent {
    /** @param {{calendarId?:string,subject?:string,body?:string,start?:string,end?:string,attendeeEmails?:string[]}} [init] */
    constructor(init) { Object.assign(this, init) }
    /** @type {string} */
    calendarId;
    /** @type {string} */
    subject;
    /** @type {string} */
    body;
    /** @type {string} */
    start;
    /** @type {string} */
    end;
    /** @type {string[]} */
    attendeeEmails;
    getTypeName() { return 'CreateCalendarEvent' }
    getMethod() { return 'POST' }
    createResponse() { return new CreateCalendarEventResponse() }
}
export class SearchUsers {
    /** @param {{name?:string}} [init] */
    constructor(init) { Object.assign(this, init) }
    /** @type {string} */
    name;
    getTypeName() { return 'SearchUsers' }
    getMethod() { return 'POST' }
    createResponse() { return new SearchUsersResponse() }
}
export class GetUserSchedule {
    /** @param {{userId?:number,schedules?:string[],startTime?:string,endTime?:string,availabilityViewInterval?:number}} [init] */
    constructor(init) { Object.assign(this, init) }
    /** @type {number} */
    userId;
    /** @type {string[]} */
    schedules;
    /** @type {string} */
    startTime;
    /** @type {string} */
    endTime;
    /** @type {number} */
    availabilityViewInterval;
    getTypeName() { return 'GetUserSchedule' }
    getMethod() { return 'POST' }
    createResponse() { return new GetUserScheduleResponse() }
}
export class ListChannelsInTeam {
    constructor(init) { Object.assign(this, init) }
    getTypeName() { return 'ListChannelsInTeam' }
    getMethod() { return 'POST' }
    createResponse() { return new ListChannelsInTeamResponse() }
}
export class Hello {
    /** @param {{name?:string}} [init] */
    constructor(init) { Object.assign(this, init) }
    /** @type {?string} */
    name;
    getTypeName() { return 'Hello' }
    getMethod() { return 'POST' }
    createResponse() { return new HelloResponse() }
}
export class ListTasksForAgent {
    /** @param {{agentId?:number}} [init] */
    constructor(init) { Object.assign(this, init) }
    /** @type {number} */
    agentId;
    getTypeName() { return 'ListTasksForAgent' }
    getMethod() { return 'GET' }
    createResponse() { return new ListTasksForAgentResponse() }
}
export class ListChatHistoryForTask {
    /** @param {{taskId?:number}} [init] */
    constructor(init) { Object.assign(this, init) }
    /** @type {number} */
    taskId;
    getTypeName() { return 'ListChatHistoryForTask' }
    getMethod() { return 'GET' }
    createResponse() { return new ListChatHistoryForTaskResponse() }
}
export class CreateStoredAgent {
    /** @param {{agentType?:string}} [init] */
    constructor(init) { Object.assign(this, init) }
    /** @type {string} */
    agentType;
    getTypeName() { return 'CreateStoredAgent' }
    getMethod() { return 'POST' }
    createResponse() { return new StoredAgentData() }
}
export class UpdateAgentCommand {
    /** @param {{commandId?:number,commandResponse?:Object}} [init] */
    constructor(init) { Object.assign(this, init) }
    /** @type {number} */
    commandId;
    /** @type {?Object} */
    commandResponse;
    getTypeName() { return 'UpdateAgentCommand' }
    getMethod() { return 'POST' }
    createResponse() { return new UpdateAgentCommandResponse() }
}
export class ContinueGptAgentTask {
    /** @param {{taskId?:number,command?:AgentCommand,commandResponse?:Object,userPrompt?:string}} [init] */
    constructor(init) { Object.assign(this, init) }
    /** @type {number} */
    taskId;
    /** @type {?AgentCommand} */
    command;
    /** @type {?Object} */
    commandResponse;
    /** @type {?string} */
    userPrompt;
    getTypeName() { return 'ContinueGptAgentTask' }
    getMethod() { return 'POST' }
    createResponse() { return new ContinueGptAgentTaskResponse() }
}
export class StartGptAgentTask {
    /** @param {{prompt?:string,successCriteria?:string,agentId?:number,agentType?:string}} [init] */
    constructor(init) { Object.assign(this, init) }
    /** @type {string} */
    prompt;
    /** @type {string} */
    successCriteria;
    /** @type {?number} */
    agentId;
    /** @type {?string} */
    agentType;
    getTypeName() { return 'StartGptAgentTask' }
    getMethod() { return 'POST' }
    createResponse() { return new StartGptAgentTaskResponse() }
}
export class Authenticate {
    /** @param {{provider?:string,state?:string,oauth_token?:string,oauth_verifier?:string,userName?:string,password?:string,rememberMe?:boolean,errorView?:string,nonce?:string,uri?:string,response?:string,qop?:string,nc?:string,cnonce?:string,accessToken?:string,accessTokenSecret?:string,scope?:string,meta?:{ [index: string]: string; }}} [init] */
    constructor(init) { Object.assign(this, init) }
    /**
     * @type {string}
     * @description AuthProvider, e.g. credentials */
    provider;
    /** @type {string} */
    state;
    /** @type {string} */
    oauth_token;
    /** @type {string} */
    oauth_verifier;
    /** @type {string} */
    userName;
    /** @type {string} */
    password;
    /** @type {?boolean} */
    rememberMe;
    /** @type {string} */
    errorView;
    /** @type {string} */
    nonce;
    /** @type {string} */
    uri;
    /** @type {string} */
    response;
    /** @type {string} */
    qop;
    /** @type {string} */
    nc;
    /** @type {string} */
    cnonce;
    /** @type {string} */
    accessToken;
    /** @type {string} */
    accessTokenSecret;
    /** @type {string} */
    scope;
    /** @type {{ [index: string]: string; }} */
    meta;
    getTypeName() { return 'Authenticate' }
    getMethod() { return 'POST' }
    createResponse() { return new AuthenticateResponse() }
}
export class AssignRoles {
    /** @param {{userName?:string,permissions?:string[],roles?:string[],meta?:{ [index: string]: string; }}} [init] */
    constructor(init) { Object.assign(this, init) }
    /** @type {string} */
    userName;
    /** @type {string[]} */
    permissions;
    /** @type {string[]} */
    roles;
    /** @type {{ [index: string]: string; }} */
    meta;
    getTypeName() { return 'AssignRoles' }
    getMethod() { return 'POST' }
    createResponse() { return new AssignRolesResponse() }
}
export class UnAssignRoles {
    /** @param {{userName?:string,permissions?:string[],roles?:string[],meta?:{ [index: string]: string; }}} [init] */
    constructor(init) { Object.assign(this, init) }
    /** @type {string} */
    userName;
    /** @type {string[]} */
    permissions;
    /** @type {string[]} */
    roles;
    /** @type {{ [index: string]: string; }} */
    meta;
    getTypeName() { return 'UnAssignRoles' }
    getMethod() { return 'POST' }
    createResponse() { return new UnAssignRolesResponse() }
}
export class Register {
    /** @param {{userName?:string,firstName?:string,lastName?:string,displayName?:string,email?:string,password?:string,confirmPassword?:string,autoLogin?:boolean,errorView?:string,meta?:{ [index: string]: string; }}} [init] */
    constructor(init) { Object.assign(this, init) }
    /** @type {string} */
    userName;
    /** @type {string} */
    firstName;
    /** @type {string} */
    lastName;
    /** @type {string} */
    displayName;
    /** @type {string} */
    email;
    /** @type {string} */
    password;
    /** @type {string} */
    confirmPassword;
    /** @type {?boolean} */
    autoLogin;
    /** @type {string} */
    errorView;
    /** @type {{ [index: string]: string; }} */
    meta;
    getTypeName() { return 'Register' }
    getMethod() { return 'POST' }
    createResponse() { return new RegisterResponse() }
}
export class QueryTeamUser extends QueryDb {
    /** @param {{skip?:number,take?:number,orderBy?:string,orderByDesc?:string,include?:string,fields?:string,meta?:{ [index: string]: string; }}} [init] */
    constructor(init) { super(init); Object.assign(this, init) }
    getTypeName() { return 'QueryTeamUser' }
    getMethod() { return 'GET' }
    createResponse() { return new QueryResponse() }
}
export class QueryCalendarEvents extends QueryDb {
    /** @param {{skip?:number,take?:number,orderBy?:string,orderByDesc?:string,include?:string,fields?:string,meta?:{ [index: string]: string; }}} [init] */
    constructor(init) { super(init); Object.assign(this, init) }
    getTypeName() { return 'QueryCalendarEvents' }
    getMethod() { return 'GET' }
    createResponse() { return new QueryResponse() }
}
export class QueryStoredAgents extends QueryDb {
    /** @param {{skip?:number,take?:number,orderBy?:string,orderByDesc?:string,include?:string,fields?:string,meta?:{ [index: string]: string; }}} [init] */
    constructor(init) { super(init); Object.assign(this, init) }
    getTypeName() { return 'QueryStoredAgents' }
    getMethod() { return 'GET' }
    createResponse() { return new QueryResponse() }
}
export class QueryStoredCommands extends QueryDb {
    /** @param {{skip?:number,take?:number,orderBy?:string,orderByDesc?:string,include?:string,fields?:string,meta?:{ [index: string]: string; }}} [init] */
    constructor(init) { super(init); Object.assign(this, init) }
    getTypeName() { return 'QueryStoredCommands' }
    getMethod() { return 'GET' }
    createResponse() { return new QueryResponse() }
}
export class QueryStoredAgentTasks extends QueryDb {
    /** @param {{skip?:number,take?:number,orderBy?:string,orderByDesc?:string,include?:string,fields?:string,meta?:{ [index: string]: string; }}} [init] */
    constructor(init) { super(init); Object.assign(this, init) }
    getTypeName() { return 'QueryStoredAgentTasks' }
    getMethod() { return 'GET' }
    createResponse() { return new QueryResponse() }
}
export class UpdateAgentTask {
    /** @param {{id?:number,completed?:boolean}} [init] */
    constructor(init) { Object.assign(this, init) }
    /** @type {number} */
    id;
    /** @type {boolean} */
    completed;
    getTypeName() { return 'UpdateAgentTask' }
    getMethod() { return 'PATCH' }
    createResponse() { return new StoredAgentTask() }
}

