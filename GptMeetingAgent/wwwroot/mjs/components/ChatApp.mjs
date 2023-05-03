import {onMounted, ref} from "vue"
import {mount} from "/mjs/app.mjs"
import {
    ContinueGptAgentTask,
    ListChatHistoryForTask,
    UpdateAgentCommand,
    QueryStoredAgentTasks,
    QueryTeamUser,
    StartGptAgentTask,
    UpdateAgentTask,
    QueryCalendarEvents
} from "/mjs/dtos.mjs"
import {useClient, useMetadata} from "@servicestack/vue"

export default {
    template: `
<div class="mx-auto mt-8 grid max-w-3xl grid-cols-1 gap-6 sm:px-6 lg:max-w-7xl lg:grid-flow-col-dense lg:grid-cols-12">
    <!-- Left column -->
    <section class="lg:col-span-2 lg:col-start-1">
        <div class="bg-white shadow sm:overflow-hidden sm:rounded-lg dark:bg-black dark:text-white">
            <div class="py-4">
                <div class="px-4 sm:px-6">
                    <h2 id="notes-title" class="text-lg font-medium text-gray-900 dark:bg-black dark:text-white">Users</h2>
                    <user-panel :users="users"></user-panel>
                </div>
            </div>
        </div>
    </section>
    
    <!-- Chat-->
    <section aria-labelledby="notes-title" class="lg:col-span-7 lg:col-start-3">
        <div class="bg-white shadow sm:overflow-hidden sm:rounded-lg dark:bg-black dark:text-white mb-10">
            <div class="py-4">
                <div class="px-4 sm:px-6">
                    <h2 id="notes-title" class="text-lg font-medium text-gray-900 dark:bg-black dark:text-white">Chat<span v-if="task && task.completed"> - Completed</span></h2>
                </div>
                <gpt-chat :task="task" :chat-history="chatHistory" :loading="loading" @auto-form-success="onAutoFormSuccess"></gpt-chat>
            </div>
            <div class="bg-gray-50 px-4 py-6 sm:px-6">
                <chat-input :enable-chat-button="task == null || task.completed !== true" 
                            :enable-generated-prompt="task == null"  
                            :chat-prompt="chatPrompt" 
                            :loading="loading"
                            @chat="startChat" 
                            :users="users"></chat-input>
            </div>
        </div>
    </section>
    <!-- Right column -->
        <section class="lg:col-span-3">
            <div class="bg-white shadow sm:overflow-hidden sm:rounded-lg dark:bg-black dark:text-white">
                <div class="py-4">
                    <div class="px-4 sm:px-6">
                        <h2 id="notes-title" class="text-lg font-medium text-gray-900 dark:bg-black dark:text-white">History</h2>
                        <task-history :tasks="tasks"></task-history>
                        <upcoming-meetings :meetings="upcomingMeetings"></upcoming-meetings>
                    </div>
                </div>
            </div>
        </section>
    <notification
        :show-notification="showNotification" 
                  @close="showNotification = false" 
                  message="'Your meeting has been booked.'" 
                  title="Task completed!"></notification>
</div>
    `,
    props: ['taskId'],
    setup(props) {
        const taskId = ref(props.taskId);
        console.log(taskId.value);
        const client = useClient();
        const chatHistory = ref([]);
        const chatPrompt = ref('');
        const loading = ref(false);
        const commandHistory = ref([]);
        const task = ref(null);
        const showNotification = ref(false);
        const tasks = ref([]);
        const users = ref([]);
        const cancelTask = ref(false);
        const upcomingMeetings = ref([]);
        
        onMounted(async () => {
            let taskHistoryResponse = await client.api(new QueryStoredAgentTasks());
            if (taskHistoryResponse.succeeded) {
                tasks.value = taskHistoryResponse.response.results;

            }
            let userResponse = await client.api(new QueryTeamUser());
            if (userResponse.succeeded) {
                users.value = userResponse.response.results;
            }
            if(taskId.value != null) {
                let taskResponse = await client.api(new QueryStoredAgentTasks({id: taskId.value}));
                if (taskResponse.succeeded) {
                    task.value = taskResponse.response.results[0];
                }

                await fetchHistory(taskId.value);
            }
            let upcomingMeetingsResponse = await client.api(new QueryCalendarEvents({take:10}));
            if (upcomingMeetingsResponse.succeeded) {
                upcomingMeetings.value = upcomingMeetingsResponse.response.results;
            }
        });

        const { makeDto, apiOf } = useMetadata();

        /// Kick off a new task and conversation with the Agent.
        /// The first chat response will contain the first command to execute.
        /// If the command doesn't require confirmation, continue the chat.
        /// Here the client specifies the agentType and successCriteria for the task.
        async function startChat(prompt) {
            loading.value = true;
            if(task.value != null && task.value.id) {
                await continueChat(task.value.id, null, null, prompt);
                return;
            }
            chatHistory.value = [{content: prompt, role: 'Human'}];
            let api = await client.api(new StartGptAgentTask({
                prompt: prompt,
                agentType: 'BookingAgent',
                successCriteria: 'Help the user with their task. If the user is requesting information, be sure to speak the results.'
            }));
            if (api.succeeded) {
                let res = api.response;
                await updateTaskCommand(res);
                await fetchHistory(res.task.id);
                if(res.question) {
                    loading.value = false;
                    return
                }
                if (!requiresConfirmation(res.command)) {
                    await automaticContinue(res);
                }
            }
            loading.value = false;
        }

        /// Check if the task is completed, otherwise continue with conversation.
        async function automaticContinue(chatResponse) {
            if (chatResponse.task.completed) {
                taskComplete();
                return;
            }
            let taskId = chatResponse.task.id;
            let command = chatResponse.command;
            let commandResponse = await performCommand(command);
            setTimeout(async () => {
                await continueChat(taskId, command, commandResponse);
            }, 1000);
        }

        /// After the first chat response, update the task and command history.
        /// Continue the chat if the command doesn't require confirmation.
        /// Otherwise, wait for the user to confirm the command.
        /// ContinueChat is a recursive function that will continue until the task is completed
        /// or the user cancels the task.
        async function continueChat(taskId, command, commandResponse, userPrompt) {
            loading.value = true;
            if (cancelTask.value) {
                cancelTask.value = false;
                console.log('cancelTask', taskId, command, commandResponse)
                return;
            }
            console.log('continueChat', taskId, command, commandResponse)
            // Extract the last command from the last chat response
            let api = await client.api(new ContinueGptAgentTask(
                {
                    taskId: taskId,
                    command: command,
                    commandResponse: commandResponse,
                    userPrompt: userPrompt
                }));
            if (api.succeeded) {
                let chatResponse = api.response;
                task.value = chatResponse.task;
                if (!chatResponse.task.completed) {
                    let res = api.response;
                    await updateTaskCommand(res);
                    await fetchHistory(taskId);
                    if(res.question) {
                        loading.value = false;
                        return;
                    }
                    if (!requiresConfirmation(res.command)) {
                        await automaticContinue(res);
                    }
                }
                else {
                    taskComplete();
                }
            }
            loading.value = false;
        }

        /// Show task complete notification.
        function taskComplete() {
            console.log('Task completed!');
            showNotification.value = true;
            setTimeout(() => {
                showNotification.value = false;
            }, 5000);
        }

        /// Update the task and command history
        /// and return, this should be done after any
        /// chat interaction with the agent.
        async function updateTaskCommand(chatResponse) {
            task.value = chatResponse.task;
            let command = chatResponse.command;
            let thoughts = chatResponse.thoughts;
            if (command != null) {
                commandHistory.value.push(command);
            }
            else {
                // internal command, eg memory add
            }
            return { command, thoughts, task };
        }

        /// If the API metadata shows the use of 
        /// the ConfirmationRequired attribute, 
        /// we can automatically stop and ask the user to confirm.
        function requiresConfirmation(command) {
            console.log('requires confirm',command);
            if (!command || command.name == null)
                return false;
            let commandApi = apiOf(command.name);
            if (!commandApi || !commandApi.request)
                return false;
            let attrs = commandApi.request.attributes;
            if (!attrs)
                return false;
            let requiresConfirmation = attrs.find(x => x.name === 'ConfirmationRequired');
            return requiresConfirmation != null || command.name === 'AskQuestion';
        }

        /// Performs the command from the Agent on behalf of the user.
        /// Returns the response from the API.
        /// Response can be null if the command is not supported.
        /// Response is fed back into the Agent to continue the conversation.
        async function performCommand(command) {
            if (command == null) {
                return null;
            }

            console.log('performCommand', command);
            let result = {};
            let requestDto = makeDto(command.name, command.body);
            let api = await client.api(requestDto);
            if (api.succeeded) {
                console.log(command.name, 'Success', api.response);
                result = api.response;
            }
            else {
                // Populate the command response with the error from the API.
                console.log(command.name, 'Failed', api.error);
                result = api.error;
            }

            let updateCommand = await client.api(new UpdateAgentCommand({commandId: command.id, commandResponse: result}));
            return result;
        }

        /// The chat history also has the command from the Agent.
        async function fetchHistory(taskId) {
            let historyReq = new ListChatHistoryForTask({ taskId: taskId });
            let history = await client.api(historyReq);
            if (history.succeeded) {
                let fullChatHistory = history.response.chatHistory;
                // sort fullChatHistory by id ascending
                fullChatHistory.sort((a, b) => a.id - b.id);
                chatHistory.value = fullChatHistory;
                console.log('fullChatHistory', fullChatHistory);
                for(let i = 0; i < chatHistory.value.length; i++) {
                    let chat = chatHistory.value[i];
                    if (chat.command != null && chat.role === 'Agent') {
                        console.log('chat.command', chat.command)
                        chat.command.requestDto = makeDto(chat.command.name, chat.command.body);
                    }
                }
                await refreshTask(taskId);
            }
        }

        async function refreshTask(taskId) {
            let taskResponse = await client.api(new QueryStoredAgentTasks({id: taskId}));
            if (taskResponse.succeeded) {
                task.value = taskResponse.response.results[0];
            }
        }

        async function onAutoFormSuccess(e) {
            console.log('onAutoFormSuccess', e);
            if (e.command == null) {
                return;
            }
            let commandName = e.command.name;
            if (commandName === 'CreateCalendarEvent') {
                let request = new UpdateAgentTask({id: e.taskId, completed: true});
                let api = await client.api(request);
                if (api.succeeded) {
                    taskComplete()
                    await fetchHistory(e.taskId);
                }
            }
        }

        return {
            chatPrompt,
            chatHistory,
            startChat,
            task,
            loading,
            commandHistory,
            showNotification,
            tasks,
            users,
            onAutoFormSuccess,
            upcomingMeetings
        }
    },
}