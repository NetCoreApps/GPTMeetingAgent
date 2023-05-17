import {ref, toRef, nextTick} from "vue";
import {useClient} from "@servicestack/vue"
import {UpdateAgentTask} from "/mjs/dtos.mjs"

export default {
    template: `
<div class="px-4 py-6 sm:px-6">
    <ul role="list" class="space-y-8" v-if="chatHistory" v-cloak>
        <li v-for="chat in chatHistory">
            <div class="flex space-x-3">
                <div class="flex-shrink-0">
                    <svg v-if="chat.role == 'Agent'" class="w-8 h-8" xmlns="http://www.w3.org/2000/svg" width="1em" height="1em" viewBox="0 0 128 128"><path fill="#e06055" d="M86.05 35.16H41.64V21.19c0-6.51 5.27-11.78 11.78-11.78h20.85c6.51 0 11.78 5.27 11.78 11.78v13.97z"/><path fill="#fff" d="M55.12 11.62c-.64-1.89-4.14-.77-5.95.32s-4.34 3.97-3.43 4.7c.9.72 2.38-1.12 5.53-1.99c2.93-.81 4.38-1.47 3.85-3.03z"/><path fill="#ce453e" d="M88.73 24.77H39.01v-2.1a3.24 3.24 0 0 1 3.24-3.24H85.5a3.24 3.24 0 0 1 3.24 3.24v2.1z"/><path fill="#78a3ad" d="m119.73 58.05l2.06-45.88a2.612 2.612 0 0 1 5.22.12v58.98c0 .83-.4 1.62-1.07 2.11l-7.41 5.41l1.2-20.74zm-111.46 0L6.22 12.17a2.622 2.622 0 0 0-2.61-2.49C2.17 9.68 1 10.85 1 12.29v58.98c0 .83.4 1.62 1.07 2.11l7.41 5.41l-1.21-20.74z"/><ellipse cx="13.69" cy="66.89" fill="#e06055" rx="9.33" ry="17.34"/><defs><ellipse id="notoV1Robot0" cx="13.69" cy="66.89" rx="9.33" ry="17.34"/></defs><clipPath id="notoV1Robot1"><use href="#notoV1Robot0"/></clipPath><path fill="#ce453e" d="M12.22 49.54h2.62v34.69h-2.62z" clip-path="url(#notoV1Robot1)"/><defs><ellipse id="notoV1Robot2" cx="13.69" cy="66.89" rx="9.33" ry="17.34"/></defs><clipPath id="notoV1Robot3"><use href="#notoV1Robot2"/></clipPath><path fill="#ce453e" d="M12.54 49.54h2.62v34.69h-2.62z" clip-path="url(#notoV1Robot3)" transform="rotate(-180 13.845 66.886)"/><ellipse cx="114.38" cy="66.89" fill="#e06055" rx="9.33" ry="17.34"/><defs><ellipse id="notoV1Robot4" cx="114.38" cy="66.89" rx="9.33" ry="17.34"/></defs><clipPath id="notoV1Robot5"><use href="#notoV1Robot4"/></clipPath><path fill="#ce453e" d="M113.23 49.54h2.62v34.69h-2.62z" clip-path="url(#notoV1Robot5)" transform="rotate(-180 114.537 66.886)"/><defs><ellipse id="notoV1Robot6" cx="114.38" cy="66.89" rx="9.33" ry="17.34"/></defs><clipPath id="notoV1Robot7"><use href="#notoV1Robot6"/></clipPath><path fill="#ce453e" d="M112.91 49.54h2.62v34.69h-2.62z" clip-path="url(#notoV1Robot7)"/><path fill="#78a3ad" d="m111.12 35.15l-11.07-10.82a10.841 10.841 0 0 0-7.58-3.09H35.73c-2.83 0-5.55 1.11-7.58 3.09L17.08 35.15a10.816 10.816 0 0 0-3.26 7.75v54.02c0 2.92 1.18 5.71 3.26 7.75l11.07 10.82c2.03 1.98 4.74 3.09 7.58 3.09h56.74c2.83 0 5.55-1.11 7.58-3.09l11.07-10.82c2.09-2.04 3.26-4.83 3.26-7.75V42.9c0-2.91-1.18-5.71-3.26-7.75z"/><circle cx="40.28" cy="55.89" r="14.63" fill="#546e7a"/><circle cx="40.28" cy="55.89" r="9.2" fill="#b8e9f4"/><circle cx="86.96" cy="55.89" r="14.63" fill="#546e7a"/><circle cx="86.96" cy="55.89" r="9.2" fill="#b8e9f4"/><path fill="#64878e" d="M8.04 52.98S3.08 64.56 5.86 76.14l-3.49-2.55s.28-5.78 1.99-12.15c1.72-6.39 3.68-8.46 3.68-8.46zm111.92 0s4.96 11.58 2.18 23.16l3.49-2.55s-.28-5.78-1.99-12.15c-1.72-6.39-3.68-8.46-3.68-8.46z"/><path fill="#e06055" d="m61.85 65.7l-5.39 12.95c-.56 1.34.43 2.82 1.88 2.82h10.57c1.44 0 2.43-1.46 1.89-2.79l-5.18-12.95c-.68-1.69-3.07-1.71-3.77-.03z"/><path fill="#eda29b" d="M63.46 67.6c-1.09-.31-1.42 1.41-2 2.92c-.96 2.53-1.22 3.71-.69 3.9c.7.26 1.31-.54 2.25-2.83c.9-2.15 1.36-3.73.44-3.99z"/><path fill="#eceff1" d="M48.39 90.33v10.21m10.58-10.21v10.21m10.59-10.21v10.21m10.58-10.21v10.21"/><path fill="#b8e9f4" d="M81.99 100.54H46.21c-4.91 0-8.9-3.98-8.9-8.9v-1.31h53.57v1.31c0 4.91-3.98 8.9-8.89 8.9z"/><path fill="#546e7a" stroke="#546e7a" stroke-miterlimit="10" stroke-width="4" d="M82.87 100.54H45.33c-4.42 0-8.01-3.59-8.01-8.01c0-1.22.99-2.2 2.2-2.2h49.17c1.22 0 2.2.99 2.2 2.2c-.01 4.42-3.59 8.01-8.02 8.01z"/><path fill="#546e7a" d="M46.82 91.52v8.4s-9.5-.78-9.5-8.28l9.5-.12zm34.48 0v8.4s9.5-.78 9.5-8.28l-9.5-.12z"/><path fill="#b8e9f4" d="M50.71 92.47h6.2v5.96h-6.2zm9.81 0h6.2v5.96h-6.2zm9.81 0h6.2v5.96h-6.2zm10.05 5.96h-.24v-5.96h6.2c0 3.29-2.67 5.96-5.96 5.96zm-33.27 0h-.24c-3.29 0-5.96-2.67-5.96-5.96h6.2v5.96z"/><path fill="#fff" d="M118.24 202.27c.01-5.65-4.59-10.26-10.25-10.27c-2.91 0-5.53 1.21-7.4 3.16v-1.6c.02-11.3-6.13-21.82-16.2-28.07l3.93-7.16c.5-.89.61-1.92.33-2.89c-.28-.98-.92-1.79-1.85-2.3c-.55-.3-1.17-.45-1.8-.45c-1.39 0-2.67.75-3.33 1.97l-4.11 7.49c-4.27-1.57-8.78-2.37-13.48-2.38c-4.64-.01-9.13.78-13.38 2.34l-4.1-7.49a3.797 3.797 0 0 0-3.33-1.98c-.64 0-1.28.16-1.8.45c-.9.48-1.57 1.31-1.86 2.28c-.28.97-.16 2.04.32 2.88l3.92 7.17c-10.11 6.2-16.28 16.72-16.29 28.04v1.7c-.08-.09-.16-.18-.25-.26a10.182 10.182 0 0 0-7.25-3.01c-5.65-.01-10.25 4.59-10.26 10.24l-.04 31.39c-.01 5.66 4.58 10.26 10.24 10.27c2.74 0 5.32-1.06 7.26-3c.19-.18.36-.38.53-.57l-.01 4.5c-.01 2.88 1.11 5.58 3.14 7.62a10.69 10.69 0 0 0 7.6 3.16h2.69l-.02 13.86c-.01 5.65 4.59 10.26 10.24 10.27c5.65.01 10.26-4.59 10.27-10.24l.02-13.85l4.61.01l-.02 13.86c0 2.74 1.06 5.31 3 7.25c1.94 1.94 4.51 3.01 7.24 3.01c5.65.01 10.25-4.59 10.26-10.24l.01-13.85h2.71c5.93.01 10.76-4.81 10.77-10.74l.01-4.34c1.88 2.1 4.6 3.42 7.63 3.42c5.65.01 10.25-4.59 10.26-10.24l.04-31.41zM27.5 233.54c0 4.15-3.36 7.49-7.5 7.49a7.503 7.503 0 0 1-7.49-7.51l.04-31.39a7.51 7.51 0 0 1 7.51-7.49c4.14 0 7.5 3.37 7.49 7.5l-.05 31.4zm2.81-40.21c.07-11.54 7.01-21.57 17.26-26.84l-5.22-9.56c-.28-.5-.1-1.14.41-1.41c.5-.28 1.13-.09 1.41.42l5.28 9.64c4.43-1.96 9.41-3.06 14.66-3.05c5.27 0 10.24 1.12 14.69 3.1l5.3-9.65c.27-.5.9-.68 1.41-.41c.51.28.69.92.41 1.42l-5.24 9.55c10.22 5.31 17.13 15.34 17.17 26.87v.14l-67.53-.08c-.01-.05-.01-.1-.01-.14zm67.24 51.47c0 3.6-2.37 6.63-5.63 7.63c-.75.23-1.55.36-2.38.36H84.1l-.01 6.51h-.02l-.01 10.09c0 4.14-3.37 7.5-7.51 7.49c-4.13 0-7.49-3.37-7.48-7.5l.02-16.61l-10.12-.01l-.02 16.61c-.01 4.14-3.37 7.5-7.51 7.49c-4.13-.01-7.49-3.37-7.48-7.51l.01-10.1h-.01l.01-6.51l-5.44-.01c-4.42-.01-8-3.58-7.99-8.01v-5.49h.01l.05-43.17l67.02.08l-.05 43.18h.01l-.03 5.48zm10.4-3.65c-4.14 0-7.49-3.36-7.49-7.51l.04-31.39c0-4.14 3.36-7.5 7.5-7.49c4.14 0 7.49 3.37 7.48 7.51l-.03 31.39a7.5 7.5 0 0 1-7.5 7.49z"/><path fill="#7cb342" d="M108 194.76c-4.14-.01-7.5 3.35-7.5 7.49l-.04 31.39c0 4.15 3.35 7.5 7.49 7.51c4.14 0 7.5-3.34 7.5-7.49l.03-31.39c.01-4.15-3.34-7.51-7.48-7.51zm-87.95-.12c-4.14-.01-7.5 3.35-7.51 7.49l-.04 31.39c-.01 4.15 3.35 7.5 7.49 7.51c4.14.01 7.5-3.34 7.5-7.49l.05-31.4c.01-4.13-3.35-7.49-7.49-7.5zm77.79-1.23c-.04-11.53-6.95-21.56-17.17-26.87l5.24-9.55c.28-.5.1-1.14-.41-1.42c-.51-.27-1.14-.09-1.41.41l-5.3 9.65c-4.45-1.98-9.42-3.1-14.69-3.1c-5.25-.01-10.23 1.09-14.66 3.05l-5.28-9.64a1.04 1.04 0 0 0-1.41-.42c-.51.27-.7.91-.41 1.41l5.22 9.56c-10.25 5.27-17.2 15.3-17.26 26.84v.14l67.53.08c.01-.05.01-.1.01-.14zm-49.12-12.15a2.832 2.832 0 0 1-2.82-2.83c0-1.56 1.27-2.83 2.83-2.83c1.57 0 2.83 1.28 2.83 2.84c0 1.55-1.27 2.82-2.84 2.82zm30.75.03c-1.56 0-2.83-1.27-2.83-2.83a2.849 2.849 0 0 1 2.84-2.83c1.55.01 2.82 1.29 2.82 2.84c0 1.56-1.27 2.82-2.83 2.82zm18.14 14.86l-67.02-.08l-.05 43.17h-.01v5.49c-.01 4.43 3.56 8 7.99 8.01l5.44.01l-.01 6.51h.01l-.01 10.1c-.01 4.13 3.35 7.5 7.48 7.51c4.14.01 7.5-3.36 7.51-7.49l.02-16.61l10.12.01l-.02 16.61c-.01 4.13 3.35 7.51 7.48 7.5c4.14.01 7.5-3.35 7.51-7.49l.01-10.09h.02l.01-6.51h5.44c.83 0 1.63-.12 2.38-.36c3.26-1.01 5.63-4.04 5.63-7.63l.02-5.47h-.01l.06-43.19z"/><path fill="#fff" d="M48.73 175.59c-1.56 0-2.83 1.27-2.83 2.83s1.26 2.83 2.82 2.83c1.57 0 2.84-1.27 2.84-2.83a2.84 2.84 0 0 0-2.83-2.83zm30.75.03c-1.56 0-2.83 1.28-2.84 2.83c0 1.57 1.27 2.83 2.83 2.83s2.83-1.26 2.83-2.83c0-1.54-1.27-2.82-2.82-2.83z"/></svg>
                    <svg v-if="chat.role == 'Human'" class="w-8 h-8" xmlns="http://www.w3.org/2000/svg" width="1em" height="1em" viewBox="0 0 24 24"><path fill="currentColor" d="M12 22.575q-.2 0-.375-.062T11.3 22.3L9 20H5q-.825 0-1.413-.588T3 18V4q0-.825.588-1.413T5 2h14q.825 0 1.413.588T21 4v14q0 .825-.588 1.413T19 20h-4l-2.3 2.3q-.15.15-.325.213t-.375.062ZM12 12q1.45 0 2.475-1.025T15.5 8.5q0-1.45-1.025-2.475T12 5q-1.45 0-2.475 1.025T8.5 8.5q0 1.45 1.025 2.475T12 12Zm0 8.2l2.2-2.2H19v-1.15q-1.35-1.325-3.138-2.087T12 14q-2.075 0-3.863.763T5 16.85V18h4.8l2.2 2.2Z"/></svg>
                </div>
                <div class="w-full">
                    <div class="text-sm flex justify-between">
                        <a href="#" class="font-medium">{{ resolveRoleAlias(chat.role) }}</a>
                        <span class="font-small text-gray-500">{{ friendlyDate(chat.created) }}</span>                        
                    </div>
                    <div class="mt-1 text-sm w-full">
                        <p>{{chat.content}}</p>
                    </div>
                </div>
            </div>
            <div class="flex py-4">
                <command-tabs class="w-full" v-if="chat.command">
                    <command-tab title="Request">
                        <div v-if="chat.command.requestDto" class="w-full">
                            <auto-form :type="chat.command.name" v-model="chat.command.requestDto" @success="OnSuccess(chat.storedAgentTaskId, chat.command)" :allow-submit="() => !task.completed" />
                        </div>
                    </command-tab>
                    <command-tab title="Response"  v-if="chat.command && chat.command.response">
                        <div class="w-full">
                            <pre class="whitespace-pre-wrap px-2 py-2"><code lang="json">{{JSON.stringify(chat.command.response, null, 2)}}</code></pre>
                        </div>
                    </command-tab>
                </command-tabs> 
            </div>
        </li>
    </ul>
    <div role="status" v-if="loading">
        <svg aria-hidden="true" class="my-auto mx-auto w-8 h-8 mr-2 text-gray-200 animate-spin dark:text-gray-600 fill-blue-600" viewBox="0 0 100 101" fill="none" xmlns="http://www.w3.org/2000/svg">
            <path d="M100 50.5908C100 78.2051 77.6142 100.591 50 100.591C22.3858 100.591 0 78.2051 0 50.5908C0 22.9766 22.3858 0.59082 50 0.59082C77.6142 0.59082 100 22.9766 100 50.5908ZM9.08144 50.5908C9.08144 73.1895 27.4013 91.5094 50 91.5094C72.5987 91.5094 90.9186 73.1895 90.9186 50.5908C90.9186 27.9921 72.5987 9.67226 50 9.67226C27.4013 9.67226 9.08144 27.9921 9.08144 50.5908Z" fill="currentColor"/>
            <path d="M93.9676 39.0409C96.393 38.4038 97.8624 35.9116 97.0079 33.5539C95.2932 28.8227 92.871 24.3692 89.8167 20.348C85.8452 15.1192 80.8826 10.7238 75.2124 7.41289C69.5422 4.10194 63.2754 1.94025 56.7698 1.05124C51.7666 0.367541 46.6976 0.446843 41.7345 1.27873C39.2613 1.69328 37.813 4.19778 38.4501 6.62326C39.0873 9.04874 41.5694 10.4717 44.0505 10.1071C47.8511 9.54855 51.7191 9.52689 55.5402 10.0491C60.8642 10.7766 65.9928 12.5457 70.6331 15.2552C75.2735 17.9648 79.3347 21.5619 82.5849 25.841C84.9175 28.9121 86.7997 32.2913 88.1811 35.8758C89.083 38.2158 91.5421 39.6781 93.9676 39.0409Z" fill="currentFill"/>
        </svg>
        <span class="sr-only">Loading...</span>
    </div>
        <notification
        :show-notification="showNotification && task && task.completed == true" 
                  @close="showNotification = false" 
                  message="'Your meeting has been booked.'" 
                  title="Task completed!"></notification>
</div>`
    ,
    emits: ['autoFormSuccess'],
    props: ['chatHistory', 'loading', 'task'],
    setup(props, context) {
        const chatHistory = toRef(props, 'chatHistory');
        const loading = toRef(props, 'loading');
        const task = toRef(props, 'task');
        const nameMapping = {
            'Agent': 'Bookings Assistant Agent',
            'Human': 'Me'
        }
        const showNotification = ref(false);

        const client = useClient();
        
        nextTick(async () => {
            if(!chatHistory.value || chatHistory.value.length === 0) {
                chatHistory.value.push({
                    role: 'Agent',
                    content: 'Hi, I\'m the Bookings Assistant Agent. How can I help you?',
                })
            }
        });

        function resolveRoleAlias(role) {
            return nameMapping[role] || role;
        }

        function resolveUserImg(roleName) {
            let mapping = {
                'Human': '/img/person-pin-rounded.svg',
                'Agent': '/img/robot.svg',
            };
            return mapping[roleName];
        }

        function friendlyDate(dateStr) {
            if (!dateStr)
                return '';
            const options = {year: 'numeric', month: 'long', day: 'numeric'};
            return new Intl.DateTimeFormat('en-US', options).format(new Date(dateStr));
        }

        const OnSuccess = async (taskId, command) => {
            context.emit('autoFormSuccess', {taskId: taskId, command: command});
        }


        return {
            chatHistory,
            loading,
            resolveRoleAlias,
            resolveUserImg,
            friendlyDate,
            task,
            OnSuccess,
            showNotification
        };
    }
}