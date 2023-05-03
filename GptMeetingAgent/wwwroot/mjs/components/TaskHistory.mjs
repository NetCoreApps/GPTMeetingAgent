import {ref, toRef, computed} from 'vue';

export default {
    template: `
<div class="">
    <ul class="divide-y divide-gray-200">
        <li v-for="task in displayedTasks" class="py-4">
            <div class="flex items-center space-x-4">
            <a :href="'/tasks/' + task.id" class="text-xs text-left" :title="task.prompt">
                <svg class="w-5 h-5 inline" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24">
                    <path fill="currentColor" d="M2 19.575V4q0-.825.588-1.413T4 2h16q.825 0 1.413.588T22 4v12q0 .825-.588 1.413T20 18H6l-2.3 2.3q-.475.475-1.088.213T2 19.575Zm2-2.4L5.175 16H20V4H4v13.175ZM4 4v13.175V4Z"/>
                </svg>
                {{task.name ? task.name : truncatePrompt(task.prompt)}}
                <span v-if="task.completed" class="mt-2 inline-flex items-center rounded-md bg-green-50 px-2 py-1 text-xs font-medium text-green-700 ring-1 ring-inset ring-green-600/20">Completed</span>
                </a>
            </div>
        </li>
        <li v-if="!displayedTasks || displayedTasks.length == 0" class="py-4">
            <div class="flex items-center space-x-4">
                <span class="text-xs text-left">No task history</span>
            </div>
        </li>
    </ul>
</div>
    `,
    props:['tasks'],
    setup(props) {
        const tasks = toRef(props,'tasks');
        const displayedTasks = computed(() => {
            return tasks.value.sort((a, b) => b.id - a.id).slice(0, 10);
        });
        
        function truncatePrompt(prompt) {
            if (prompt.length > 35) {
                return prompt.substring(0,35) + '...';
            }
            return prompt;
        }
        
        return {tasks, displayedTasks, truncatePrompt};
    }
}