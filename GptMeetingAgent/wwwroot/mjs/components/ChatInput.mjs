import {toRef, ref, watch} from "vue";

export default {
    template: `
<div class="flex space-x-3">
    <div class="flex-shrink-0">
        <img class="h-10 w-10 rounded-full" src="/img/person-pin-rounded.svg" alt="">
    </div>
    <div class="min-w-0 flex-1">
        <form v-on:submit.prevent="chatAction">
            <div>
                <textarea v-model="chatPrompt" rows="3" class="text-black block w-full placeholder:text-gray-400 focus:ring-2 focus:ring-inset focus:ring-blue-600 sm:py-1.5 sm:text-sm sm:leading-6" placeholder="I need to book a meeting..."></textarea>
            </div>
            <div class="mt-3 flex items-center justify-between">
                <button v-if="enableGeneratedPrompt" type="button" :disabled="loading" @click="generateExamplePrompt" class="rounded-md bg-blue-600 px-3 py-2 text-sm font-semibold text-white shadow-sm hover:bg-blue-500 focus-visible:outline focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-blue-600">Generate Prompt</button>
                <button v-if="enableChatButton" type="submit" :disabled="loading" class="rounded-md bg-blue-600 px-3 py-2 text-sm font-semibold text-white shadow-sm hover:bg-blue-500 focus-visible:outline focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-blue-600">Chat</button>
            </div>
        </form>
    </div>
</div>`,
    emit:['chat'],
    props:['users','chatPrompt', 'enableGeneratedPrompt', 'enableChatButton', 'loading'],
    setup(props, context) {
        const users = toRef(props,'users');
        const chatPrompt = ref('');
        const enableGeneratedPrompt = toRef(props,'enableGeneratedPrompt');
        const enableChatButton = toRef(props,'enableChatButton');
        const chatPromptRo = toRef(props,'chatPrompt');
        const loading = toRef(props,'loading');
        watch(chatPromptRo, (value) => {
            chatPrompt.value = chatPromptRo.value;
        });
        function chatAction() {
            console.log('chatAction', chatPrompt.value);
            context.emit('chat', chatPrompt.value);
            chatPrompt.value = '';
        }
        const topics = ['YouTube Video Script','Code Review', 'Product Roadmap', 'Sales Report', 'Marketing Plan', 'Customer Feedback', 'Sales Report', 'Marketing Plan', 'Customer Feedback'];
        const locations = ['Meeting Room 1', 'Meeting Room 2', 'Meeting Room 3', 'Meeting Room 4', 'Meeting Room 5',
            'Conference Room 1', 'Conference Room 2', 'Conference Room 3', 'Conference Room 4', 'Conference Room 5'];
        const dates = generateAWeekOfDays();
        const times = ['afternoon', 'morning', 'evening'];

        function generateAWeekOfDays() {
            let dates = [];
            let days = ['Sunday', 'Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday'];
            let now = Date.now();
            for (let i = 0; i < 7; i++) {
                let day = new Date(now + (i * 24 * 60 * 60 * 1000)).getDay();
                let date = new Date(now + (i * 24 * 60 * 60 * 1000));
                dates.push({ day: days[day], date: date });
            }

            return dates;
        }

        function generateExamplePrompt() {
            const randomUserIndex = Math.floor(Math.random() * users.value.length);
            const randomTopic = Math.floor(Math.random() * topics.length);
            const randomLocation = Math.floor(Math.random() * locations.length);
            const randomDate = Math.floor(Math.random() * dates.length);
            const randomTime = Math.floor(Math.random() * times.length);
            chatPrompt.value = `I want to have a meeting with ${users.value[randomUserIndex].displayName.split(' ')[0]} regarding ${topics[randomTopic]} on ${dates[randomDate].day} the ${addSuffix(dates[randomDate].date.getDate())} sometime in the ${times[randomTime]}.`;
        }

        function addSuffix(day) {
            if (day >= 11 && day <= 13) {
                return day + "th";
            }
            switch (day % 10) {
                case 1:
                    return day + "st";
                case 2:
                    return day + "nd";
                case 3:
                    return day + "rd";
                default:
                    return day + "th";
            }
        }
        return {users, chatPrompt, generateExamplePrompt, enableGeneratedPrompt,
            enableChatButton, chatAction, loading};
    }
}